"""Assemble the 2,304 Unity samples. Requires Pillow, NumPy and FFmpeg."""
import itertools
import json
import subprocess
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
FRAMES = ROOT / "Library/HumanoidFramesV02"
OUT = ROOT / "Docs/Development/Benchmarks/HumanoidAssemblyV02"
ART = ROOT / "ArtSource/References/Approved/World/SN_Concept_Scene_01.png"
MOTIONS = {"Idle": 2.0, "Walk": 1.0, "Work": 1.5}
VARIANTS = ["".join(map(str, bits)) for bits in itertools.product(range(2), repeat=4)]
SIZES = (64, 96, 160)
COLORS = ("#e2dcd1", "#242326")
CELL = (150, 188)
GAME = Image.open(ART).convert("RGB")
GAME = GAME.crop((570, 480, 990, 900)).resize(CELL, Image.Resampling.LANCZOS)

def read(motion, flags, phase):
    with Image.open(FRAMES/f"{motion}-{flags}-{phase:02}.png") as im:
        rgb = np.asarray(im.convert("RGB"), dtype=np.int16)
    # The matte is flat green; the approved sprites contain no saturated green.
    surplus = np.maximum(0, rgb[:,:,1] - np.maximum(rgb[:,:,0], rgb[:,:,2]))
    alpha = np.clip(255-surplus,0,255)
    alpha[alpha < 5] = 0
    rgb[:,:,1] -= surplus
    rgba = np.dstack((rgb,alpha)).astype(np.uint8)
    figure = Image.fromarray(rgba)
    bounds = figure.getchannel("A").getbbox()
    if not bounds or min(bounds[:2]) == 0 or bounds[2] >= im.width or bounds[3] >= im.height:
        raise ValueError((motion,flags,phase,bounds))
    return figure

def tile(figure, height, background):
    # Fixed camera registration through each loop; do not normalise away the walk bounce.
    scale = height / neutral_height
    figure = figure.resize((round(figure.width*scale),round(figure.height*scale)),Image.Resampling.LANCZOS)
    cell = GAME.copy() if background == 2 else Image.new("RGB",CELL,COLORS[background])
    cell.paste(figure,((CELL[0]-figure.width)//2,(CELL[1]-figure.height)//2),figure)
    return cell

first = read("Idle","0010",0)
bounds = first.getchannel("A").getbbox()
neutral_height = bounds[3]-bounds[1]
print("Camera reference height:",neutral_height,flush=True)
# Every volume of the matrix has an explicit facing, coat, boots and scarf bit.
# The video repeats each clip at its duration and cycles the viewing conditions.
video_size = (CELL[0]*8, 2*CELL[1]+58)
encoder = subprocess.Popen(["ffmpeg","-loglevel","error","-y","-f","rawvideo","-pix_fmt","rgb24",
    "-s",f"{video_size[0]}x{video_size[1]}","-r","48","-i","-","-an","-c:v","libx264",
    "-crf","18","-pix_fmt","yuv420p","-movflags","+faststart",str(OUT/"cycles.mp4")],stdin=subprocess.PIPE)
metrics = {}
try:
    for motion,duration in MOTIONS.items():
        # Decode once per clip. Small crops retain the camera origin via their placement.
        phases=[[read(motion,key,i) for key in VARIANTS] for i in range(48)]
        min_mirror = 1.0
        min_changed = {flag: float("inf") for flag in (1,2,4)}
        for variants in phases:
            for k in range(8):
                right = np.asarray(variants[k])
                left = np.asarray(variants[k+8])[:,::-1]
                a,b = right[:,:,3]>128,left[:,:,3]>128
                score = np.count_nonzero(a & b) / np.count_nonzero(a | b)
                min_mirror = min(min_mirror,score)
                for flag in min_changed:
                    other = np.asarray(variants[k ^ flag])
                    delta = np.max(np.abs(right.astype(np.int16)-other),axis=2)
                    min_changed[flag] = min(min_changed[flag],int(np.count_nonzero(delta>24)))
        if min_mirror < .94 or min(min_changed.values()) < 20:
            raise ValueError((motion,min_mirror,min_changed))
        print(motion,"min mirror IoU",round(min_mirror,4),"min changed px (scarf/boots/coat)",min_changed,flush=True)
        metrics[motion] = {"phases": 48, "variants": len(VARIANTS), "mirror_iou_min": float(min_mirror),
                           "min_changed_pixels": dict(zip(("scarf","boots","coat"),min_changed.values()))}
        for repeat in range(9):
            for j in range(round(duration*48)):
                phase=int(j/duration)%48
                frame=Image.new("RGB",video_size,"#302e2b")
                draw=ImageDraw.Draw(frame)
                draw.text((8,4),f"{motion}   {phase+1:02}/48     rows: right / left     columns: shirt (1-4), jacket (5-8)     pairs: feet / boots     alternating: scarf off / on",fill="white")
                for k,figure in enumerate(phases[phase]):
                    col=k%8
                    height=SIZES[(col+repeat)%3]
                    background=(k//2+repeat//3)%3
                    frame.paste(tile(figure,height,background),(CELL[0]*col,24+CELL[1]*(k//8)))
                    draw.text((CELL[0]*col+5,video_size[1]-24),f"{height} px",fill="white")
                encoder.stdin.write(frame.tobytes())
        # Separate all-phase sheets make the knee, boot and sleeve inspectable.
        sheet=Image.new("RGB",(12*104,8*131),"#383631")
        for i,variants in enumerate(phases):
            for row,key in enumerate(("0110","1001")):
                pic=tile(variants[VARIANTS.index(key)],160,row).resize((104,131),Image.Resampling.LANCZOS)
                sheet.paste(pic,((i%12)*104,(row*4+i//12)*131))
        sheet.save(OUT/(motion.lower()+"-phases.jpg"),quality=91)
        print(motion,len(phases)*len(VARIANTS),"frames",flush=True)
finally:
    encoder.stdin.close()
    if encoder.wait() != 0: raise RuntimeError("FFmpeg failed")

sheet=Image.new("RGB",(CELL[0]*6,3*(CELL[1]+24)),"#383631")
for row,(motion,phase) in enumerate((("Idle",0),("Walk",12),("Work",36))):
    for col in range(6):
        flags=("0010","1111","1010","0100","0001","1101")[col]
        height=SIZES[col//2]
        im=tile(read(motion,flags,phase),height,(col+row)%3)
        x,y=col*CELL[0],row*(CELL[1]+24)
        sheet.paste(im,(x,y))
        ImageDraw.Draw(sheet).text((x+4,y+CELL[1]+4),f"{motion} {phase}/48  {height}px",fill="white")
sheet.save(OUT/"scales.jpg",quality=94)

(OUT/"capture-results.json").write_text(json.dumps(metrics,indent=2)+"\n",encoding="utf-8")
