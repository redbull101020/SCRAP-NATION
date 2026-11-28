"""Register V02 sheets to the immutable V01 joint layout. Requires Pillow."""
import json
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[2]
TOOLS = Path(__file__).resolve().parent
SOURCE = ROOT / "ArtSource/Characters/Humanoid/Generated/V02"
ART = ROOT / "Assets/_ScrapNation/Art"
OUT = ART / "HumanoidAssemblyV02"
REPORT = ROOT / "Docs/Development/Benchmarks/HumanoidAssemblyV02"
OUT.mkdir(parents=True, exist_ok=True)
REPORT.mkdir(parents=True, exist_ok=True)
baseline = json.loads((TOOLS.parent / "HumanoidAssemblyV01/layout.json").read_text())
bones = {b["name"]: complex(b["x"], b["y"]) for b in baseline["bones"]}
old = {p["name"]: p for p in baseline["parts"]}
parts = []
layers = {}
sheets = {p.stem: Image.open(p).convert("RGBA") for p in SOURCE.rglob("*.png")}
canvas = (543, 724)

# Source landmarks are recorded on previews 400 px wide (383 for Legs).
# Local longitudinal scale follows two landmarks; width is fitted independently
# to the approved projected anatomy. The output records the centre registration.
def module(name, source, region, start, end, joint, target, width, order, enabled):
    key = "SN_Humanoid_" + source
    sheet = sheets[key]
    ratio = sheet.width / (383 if source.startswith("Legs") else 400)
    box = tuple(round(v * ratio) for v in region)
    tile = sheet.crop(box)
    alpha = tile.getchannel("A")
    core = alpha.point(lambda a: 255 if a >= 80 else 0)
    seed = tuple(round(v * ratio) - origin for v, origin in zip(start, box))
    if not core.getpixel(seed):
        raise ValueError((name, seed))
    ImageDraw.floodfill(core, seed, 128)
    # Only admit faint pixels within three original pixels of the connected ink.
    keep = core.point(lambda a: 255 if a == 128 else 0).filter(ImageFilter.MaxFilter(7))
    tile.putalpha(ImageChops.multiply(keep, alpha))
    p = complex(*start) * ratio - complex(*box[:2])
    axis = (complex(*end) - complex(*start)) * ratio
    dest = bones[joint]
    travel = (bones[target] if isinstance(target, str) else complex(*target)) - dest

    def inverse(z):
        along = (z - dest) / travel
        return p + axis * complex(along.real, along.imag / width)

    origin = inverse(0)
    dx, dy = inverse(1) - origin, inverse(1j) - origin
    transform = (dx.real, dy.real, origin.real, dx.imag, dy.imag, origin.imag)
    image = tile.transform(canvas, Image.Transform.AFFINE, transform, Image.Resampling.BICUBIC)
    if name.startswith(("Thigh_", "Shin_")):
        # Fade the distal rim into the next painted segment at knee and ankle.
        alpha = image.getchannel("A")
        l,t,r,b = alpha.getbbox()
        for y in range(t,b):
            for x in range(l,r):
                along = ((complex(x,y)-dest)/travel).real
                fade = max(0, min(1, (1.10-along)/0.12))
                if fade < 1:
                    alpha.putpixel((x,y),round(alpha.getpixel((x,y))*fade))
        image.putalpha(alpha)
    if name.startswith("Foot_"):
        # The socket drawn at the top of each bare foot is wider than the ankle.
        # Confine its upper lip to the shin, easing back to the painted foot below.
        alpha = image.getchannel("A")
        shin_mask = layers[name.replace("Foot_", "Shin_")].getchannel("A")
        l,t,r,b = alpha.getbbox()
        for y in range(t,min(b,int(dest.imag)+12)):
            blend = max(0,min(1,(y-dest.imag)/12))
            for x in range(l,r):
                limit = shin_mask.getpixel((x,y))/255
                alpha.putpixel((x,y), round(alpha.getpixel((x,y))*(limit+(1-limit)*blend)))
        image.putalpha(alpha)
    l,t,r,b = image.getchannel("A").getbbox()
    image.crop((l,t,r,b)).save(OUT / (name+".png"))
    parts.append(dict(name=name, joint=joint, order=order, enabled=enabled,
                      offsetX=(l+r)/2-dest.real, offsetY=dest.imag-(t+b)/2))
    layers[name] = image

for p in baseline["parts"]:
    if p["name"] in {"Neck", "Torso", "Pelvis", "Scarf", "Head"}:
        parts.append(dict(p, shared=True))

for side in ("Near", "Far"):
    near = side == "Near"
    thigh, shin, foot, upper, fore, hand = [f"{n}_{side}" for n in
        ("Thigh","Shin","Foot","UpperArm","Forearm","Hand")]
    def limb(name, sheet, region, pins, joint, target, width):
        spec = old[name]
        order = spec["order"] - 2 if name.startswith("Foot_") else spec["order"]
        module(name,sheet,region,*pins,joint,target,width,order,spec["enabled"])

    limb(thigh,"Legs_V02",(10,3,85,137) if near else (12,141,81,268),
         ((55,29),(36,110)) if near else ((43,169),(56,242)),thigh,shin,1.04)
    limb(shin,"Legs_V02",(105,2,170,139) if near else (105,142,163,270),
         ((142,24),(127,119)) if near else ((134,166),(129,247)),shin,foot,.65)
    limb(foot,"Legs_V02",(177,42,261,137) if near else (180,185,263,266),
         ((202,66),(244,118)) if near else ((203,210),(245,249)),foot,
         (234,678) if near else (369,677),.87)
    boot = "Boot_"+side
    limb(boot,"Legs_V02",(273,24,370,141) if near else (280,174,375,273),
         ((311,65),(348,117)) if near else ((313,207),(354,252)),foot,
         (235,679) if near else (366,677),.85)
    limb(upper,"Arms_V02",(2,4,56,87) if near else (56,4,98,87),
         ((36,29),(20,71)) if near else ((75,28),(79,70)),upper,fore,.65)
    limb(fore,"Arms_V02",(3,90,48,193) if near else (49,89,91,193),
         ((32,106),(26,178)) if near else ((72,106),(60,178)),fore,hand,.98)
    limb(hand,"Arms_V02",(5,194,54,265) if near else (54,194,94,265),
         ((28,207),(34,251)) if near else ((71,209),(68,250)),hand,
         (190,417) if near else (373,416),.88)
    sleeve = "Jacket_Sleeve_"+side
    module(sleeve,"Clothing_V02",(197,47,288,200) if near else (302,48,385,201),
           (244,87) if near else (342,86),(232,173) if near else (350,173),
           upper,(188,265) if near else (332,265),.99,45 if near else 11,False)

# Scarf-free fleece vest from equipment atlas 02.
module("Jacket_Torso","EquipmentAtlas_V02_02",(236,67,303,135),
       (271,122),(268,78),"Spine",(269,183),.88,40,False)
(TOOLS/"layout.json").write_text(json.dumps({"parts":parts},indent=2)+"\n",encoding="utf-8")

def neutral(specs, jacket=False, scarf=False, boots=True):
    image=Image.new("RGBA",canvas)
    for p in sorted(specs,key=lambda p:p["order"]):
        name=p["name"]
        visible=p["enabled"]
        if name=="Torso": visible=not jacket
        if name.startswith("Jacket"): visible=jacket
        if name=="Scarf": visible=scarf
        if name.startswith("Boot_"): visible=boots
        if name.startswith("Foot_"): visible=not boots
        if not visible: continue
        if specs is parts and name in layers: image.alpha_composite(layers[name]); continue
        sprite=Image.open(ART/"HumanoidAssemblyV01"/(name+".png"))
        pivot=bones[p["joint"]]
        xy=(round(pivot.real+p["offsetX"]-sprite.width/2),round(pivot.imag-p["offsetY"]-sprite.height/2))
        image.alpha_composite(sprite,xy)
    return image

board=Image.new("RGB",(4*360,510),"#b6b0a8")
reference=Image.open(ROOT/"ArtSource/References/Approved/Characters/SN_Humanoid_Anatomy_V01.png")
for i,(label,img) in enumerate([("Approved anatomy",reference),("V01",neutral(baseline["parts"])),
        ("V02 / boots",neutral(parts)),("V02 / jacket / bare feet",neutral(parts,True,False,False))]):
    im=img.resize((360,480),Image.Resampling.LANCZOS)
    board.paste(im,(360*i,22),im)
    ImageDraw.Draw(board).text((360*i+15,5),label,fill="black")
board.save(REPORT/"neutral.jpg",quality=94)

# Candidate comparison before committing to the combined-head fallback.
head_sheet = sheets["SN_Humanoid_Head_V02"]
head_board = Image.new("RGB",(800,219),"#b6b0a8")
candidates = [("V01 / retained",Image.open(ART/"HumanoidAssemblyV01/Head.png")),
              ("V02 / candidate",head_sheet.crop((220,0,468,235))),
              ("Hair candidate",head_sheet.crop((30,225,310,445))),
              ("Goggles candidate",head_sheet.crop((172,425,425,571)))]
for i,(label,part) in enumerate(candidates):
    if label == "Hair candidate":
        mask = part.getchannel("A").point(lambda a: 255 if a >= 80 else 0)
        ImageDraw.floodfill(mask,(140,86),128)
        keep = mask.point(lambda a: 255 if a == 128 else 0).filter(ImageFilter.MaxFilter(5))
        part.putalpha(ImageChops.multiply(keep,part.getchannel("A")))
    part.thumbnail((176,165),Image.Resampling.LANCZOS)
    head_board.paste(part,(i*200+(200-part.width)//2,20),part)
    ImageDraw.Draw(head_board).text((i*200+12,192),label,fill="black")
head_board.save(REPORT/"head-candidates.jpg",quality=94)

# Same camera-space windows for the before/after knee and ankle comparisons.
joints_board = Image.new("RGB",(800,446),"#b6b0a8")
for col,(label,specs,boots) in enumerate([("V01 / boots",baseline["parts"],True),
        ("V02 / boots",parts,True),("V01 / feet",baseline["parts"],False),("V02 / feet",parts,False)]):
    actor = neutral(specs,boots=boots)
    for y,box in [(28,(161,420,361,531)),(175,(151,558,351,703))]:
        detail = actor.crop(box)
        joints_board.paste(detail,(col*200,y),detail)
    ImageDraw.Draw(joints_board).text((col*200+7,6),label,fill="black")
joints_board.crop((0,0,800,330)).save(REPORT/"joints.jpg",quality=96)
