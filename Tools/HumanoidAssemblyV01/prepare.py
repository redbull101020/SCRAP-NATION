"""Bake rigid test sprites from the V01 originals. Run from any directory with Pillow."""
import hashlib
import json
import math
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / "ArtSource"
OUT = ROOT / "Assets/_ScrapNation/Art/HumanoidAssemblyV01"
REPORT = ROOT / "Docs/Development/Benchmarks/HumanoidAssemblyV01"
OUT.mkdir(parents=True, exist_ok=True)
REPORT.mkdir(parents=True, exist_ok=True)
for line in (ART / "SHA256SUMS.txt").read_text().splitlines():
    digest, path = line.split(None, 1)
    assert hashlib.sha256((ART / path).read_bytes()).hexdigest() == digest, path

# Sheet coordinates are measured on a 380px-wide proxy. All sampling uses the full sheet.
sheets = {k: Image.open(next(ART.rglob("SN_Humanoid_" + k + "_V01.png"))).convert("RGBA")
          for k in ("Core", "Arms", "Legs", "Anatomy")}
reference = sheets["Anatomy"].resize((543, 724), Image.Resampling.LANCZOS)
bones = {}
def bone(name, parent, x, y):
    bones[name] = dict(name=name, parent=parent, x=x, y=y)
bone("Pelvis", "", 273, 348)
bone("Spine", "Pelvis", 273, 316)
bone("Neck", "Spine", 266, 171)
bone("Head", "Neck", 265, 139)
for side, coords in {
    "Near": [(215,200),(181,278),(172,360),(190,428),(247,358),(226,463),(195,616),(234,684)],
    "Far": [(308,201),(337,280),(364,358),(373,424),(291,359),(306,467),(308,615),(373,680)]
}.items():
    for names, points, parent in [(("UpperArm","Forearm","Hand"),coords[:3],"Spine"),
                                   (("Thigh","Shin","Foot"),coords[4:7],"Pelvis")]:
        for part, (x,y) in zip(names,points):
            name = part+"_"+side
            bone(name,parent,x,y)
            parent=name
def at(name):
    b=bones[name]
    return (b["x"], b["y"])

parts=[]
layers=[]
def record(name, joint, image, order, enabled=True):
    bounds=image.getchannel("A").getbbox()
    assert bounds, name
    image.crop(bounds).save(OUT/(name+".png"))
    x,y=at(joint)
    l,t,r,b=bounds
    parts.append(dict(name=name,joint=joint,order=order,enabled=enabled,
                      offsetX=(l+r)/2-x,offsetY=y-(t+b)/2))
    if enabled: layers.append((order,image))

def cut(name, sheet, region, pins, joint, target, order, enabled=True):
    src=sheets[sheet]
    factor=src.width/380
    l,t,r,b=[round(v*factor) for v in region]
    crop=src.crop((l,t,r,b))
    # Retain the connected island nearest the crop centre; reject detached background.
    alpha=crop.getchannel("A")
    mask=alpha.point(lambda a: 255 if a>=50 else 0)
    cx,cy=crop.width//2,crop.height//2
    seed=min(((x,y) for y in range(0,crop.height,3) for x in range(0,crop.width,3)
              if mask.getpixel((x,y))),key=lambda q:(q[0]-cx)**2+(q[1]-cy)**2)
    ImageDraw.floodfill(mask,seed,128)
    mask=mask.point(lambda v:255 if v==128 else 0).filter(ImageFilter.MaxFilter(7))
    from PIL import ImageChops
    crop.putalpha(ImageChops.multiply(mask,alpha.point(lambda v:max(0,min(255,round((v-12)*255/226))))))
    # Two landmarks define one similarity transform: uniform scale + rotation, no shear.
    p,q=[complex(x*factor-l,y*factor-t) for x,y in pins]
    a=complex(*at(joint)); dest=complex(*target)
    inv=(q-p)/(dest-a)
    offset=p-inv*a
    image=crop.transform(reference.size,Image.Transform.AFFINE,
                         (inv.real,-inv.imag,offset.real,inv.imag,inv.real,offset.imag),
                         Image.Resampling.BICUBIC)
    record(name,joint,image,order,enabled)

cut("Neck","Core",(229,70,262,103),[(247,96),(247,77)],"Neck",(265,129),44)
cut("Torso","Core",(0,109,55,185),[(27,173),(29,120)],"Spine",(269,176),40)
cut("Jacket","Core",(110,107,173,186),[(141,174),(143,119)],"Spine",(269,176),40,False)
cut("Pelvis","Core",(10,189,71,247),[(40,212),(40,236)],"Pelvis",(273,397),30)
cut("Scarf","Core",(296,69,336,110),[(314,78),(316,100)],"Neck",(270,208),55,False)
# The approved head preserves silhouette, face, hair and goggles as one temporary module.
mask=Image.new("L",reference.size)
ImageDraw.Draw(mask).polygon([(175,0),(363,0),(364,145),(303,147),(278,136),
                             (258,125),(254,143),(222,150),(180,135)],fill=255)
from PIL import ImageChops
head=reference.copy()
head.putalpha(ImageChops.multiply(mask,head.getchannel("A").point(lambda a:0 if a<10 else a)))
record("Head","Head",head,52)
for side, near in [("Near",True),("Far",False)]:
    upper,fore,hand,thigh,shin,foot=[n+"_"+side for n in ("UpperArm","Forearm","Hand","Thigh","Shin","Foot")]
    specs=[
        (upper,"Arms",(40,16,111,117) if near else (47,134,102,234),
         [(81,37),(60,102)] if near else [(72,147),(82,217)],at(fore),42 if near else 8),
        (fore,"Arms",(160,16,215,118) if near else (161,137,222,233),
         [(183,37),(196,101)] if near else [(179,155),(201,216)],at(hand),43 if near else 9),
        (hand,"Arms",(262,52,330,124) if near else (266,165,327,235),
         [(291,65),(306,111)] if near else [(293,177),(305,219)],(190,421) if near else (374,418),44 if near else 10),
        (thigh,"Legs",(22,5,95,125) if near else (24,132,94,243),
         [(63,31),(49,111)] if near else [(54,153),(63,224)],at(shin),26 if near else 15),
        (shin,"Legs",(111,15,165,122) if near else (112,141,164,239),
         [(142,38),(130,106)] if near else [(140,160),(141,221)],at(foot),24 if near else 13),
        (foot,"Legs",(190,59,260,124) if near else (190,172,252,238),
         [(211,75),(246,109)] if near else [(210,190),(239,224)],(231,680) if near else (365,676),25 if near else 14),
        ("Boot_"+side,"Legs",(276,26,360,125) if near else (278,150,354,241),
         [(309,47),(340,109)] if near else [(306,171),(337,226)],(232,683) if near else (367,680),25 if near else 14)]
    for name,sheet,region,pins,target,order in specs:
        is_boot=name.startswith("Boot")
        cut(name,sheet,region,pins,foot if is_boot else name,target,order,name!=foot)
# Keep build data outside Assets; importer and baker use this same measured coordinate frame.
config=dict(ppu=360,bones=list(bones.values()),parts=parts)
(Path(__file__).parent/"layout.json").write_text(json.dumps(config,indent=2)+"\n",encoding="utf-8")
neutral=Image.new("RGBA",reference.size)
for _,layer in sorted(layers,key=lambda item:item[0]): neutral.alpha_composite(layer)
markers=neutral.copy()
draw=ImageDraw.Draw(markers)
for b in bones.values():
    x,y=at(b["name"])
    if b["parent"]: draw.line([at(b["parent"]),(x,y)], fill="#70e6ce",width=2)
    draw.ellipse((x-4,y-4,x+4,y+4),fill="#ffb652")
canvas=Image.new("RGB",(543*3,760),"#c2b9ae")
for i,(im,label) in enumerate([(reference,"Approved anatomy"),(neutral,"Rigid assembly V01"),(markers,"Bone pivots")]):
    canvas.paste(im,(543*i,30),im)
    ImageDraw.Draw(canvas).text((543*i+24,8),label,fill="black")
canvas.save(REPORT/"neutral.jpg",quality=89)
