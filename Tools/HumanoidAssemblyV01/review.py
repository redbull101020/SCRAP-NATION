"""Build review media from the Unity capture (Pillow + FFmpeg on PATH)."""
from pathlib import Path
import subprocess
from PIL import Image, ImageDraw, ImageChops

root=Path(__file__).resolve().parents[2]
frames=root/"Library/HumanoidAssemblyFrames"
out=root/"Docs/Development/Benchmarks/HumanoidAssemblyV01"
def read(motion,variant,i):
    return Image.open(frames/f"{motion}-{variant}-{i:02}.png").convert("RGB")
def silhouette(im):
    delta=ImageChops.difference(im,Image.new("RGB",im.size,im.getpixel((0,0))))
    return delta.convert("L").point(lambda v:255 if v>14 else 0)
def label(canvas, xy, text):
    ImageDraw.Draw(canvas).text(xy,text,fill="#f2e8d7")
motions=[("Idle",2.0),("Walk",1.0),("Work",1.5)]
sheet=Image.new("RGB",(920,590),"#342e2b")
for row,(motion,seconds) in enumerate(motions):
    label(sheet,(8,195*row+4),motion+"   |   right: shirt + boots        left: jacket + scarf + feet")
    for col,(height,idx) in enumerate([(64,0),(96,12),(160,32)]):
        for variant in (0,1):
            im=read(motion,variant,idx)
            bounds=silhouette(im).getbbox()
            figure=im.crop(bounds)
            width=round(figure.width*height/figure.height)
            x=col*306+variant*137+19
            sheet.paste(figure.resize((width,height),Image.Resampling.LANCZOS),(x,195*row+179-height))
        label(sheet,(col*306+20,195*row+180),str(height)+" px")
sheet.save(out/"scales.jpg",quality=91)
# Uncut normalized-phase sampling, retimed to each imported clip's actual duration.
proc=subprocess.Popen(["ffmpeg","-loglevel","error","-y","-f","rawvideo","-pix_fmt","rgb24",
                       "-s","720x292","-r","48","-i","-","-an","-c:v","libx264",
                       "-crf","17","-pix_fmt","yuv420p","-movflags","+faststart",str(out/"cycles.mp4")],stdin=subprocess.PIPE)
for motion,duration in motions:
    for j in range(round(48*duration)*2):
        phase=(j/(48*duration))%1; i=int(phase*48)%48
        canvas=Image.new("RGB",(720,292),"#342e2b")
        for k,(variant,desc) in enumerate([(0,"Base / right"),(1,"Changed / left"),(int(phase>=.5),"Swap + turn at 50%")]):
            canvas.paste(read(motion,variant,i),(k*240,20))
            label(canvas,(k*240+8,267),desc)
        label(canvas,(8,3),f"{motion}    {duration:g} s    phase {i+1}/48")
        proc.stdin.write(canvas.tobytes())
proc.stdin.close()
assert proc.wait()==0
overview=Image.new("RGB",(12*80,24*80),"#aaa39d")
for n,(motion,_) in enumerate(motions):
    for v in (0,1):
        gaps=[]
        for i in range(48):
            im=read(motion,v,i)
            overview.paste(im.resize((80,80)),((i%12)*80,(8*n+4*v+i//12)*80))
            mask=silhouette(im)
            areas=[]
            while (box:=mask.getbbox()):
                y=box[1]; x=next(x for x in range(box[0],box[2]) if mask.getpixel((x,y)))
                ImageDraw.floodfill(mask,(x,y),128)
                areas.append(mask.histogram()[128])
                mask=mask.point(lambda s:255 if s==255 else 0)
            if sum(a>=10 for a in areas)>1: gaps.append(i)
        print(motion,v,"frames with separated regions >=10 px:",gaps)
overview.save(frames/"all-phases.jpg",quality=88)
