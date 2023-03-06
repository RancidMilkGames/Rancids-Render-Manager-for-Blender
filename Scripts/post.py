import bpy
import os

scene = bpy.context.scene

path = TARGETDIRECTORY
extension = EXTENSION
files = os.listdir(path)
files.sort()

# create the sequencer data
scene.sequence_editor_create()

seq = scene.sequence_editor.sequences.new_image(
		name = 'MyStrip',
		filepath=os.path.join(path, files[0]),
		channel=1, frame_start=1)

# add the rest of the images.
for f in files:
	seq.elements.append(f)
	


# reverse if you want
seq.use_reverse_frames = False

render = scene.render
scene.frame_start = 1
scene.frame_end = 1 + len(files)

if extension == 'MP4':
	bpy.context.scene.render.image_settings.file_format = 'FFMPEG'
	bpy.context.scene.render.ffmpeg.format = 'MPEG4'
	bpy.context.scene.render.ffmpeg.codec = 'H264'
	
elif extension == 'THEORA/OGG':
	bpy.context.scene.render.image_settings.file_format = 'FFMPEG'
	bpy.context.scene.render.ffmpeg.format = 'OGG'
	bpy.context.scene.render.ffmpeg.codec = 'THEORA'

elif extension == 'WEBP':
	bpy.context.scene.render.image_settings.file_format = 'WEBP'
	bpy.context.scene.render.image_settings.color_mode = 'RGBA'
	bpy.context.scene.render.image_settings.quality = 100
	
elif extension == 'AVI':
	bpy.context.scene.render.image_settings.file_format = 'AVI_JPEG'
	bpy.context.scene.render.image_settings.quality = 100


if extension != 'WEBP':
	bpy.context.scene.render.filepath = FILENAME

bpy.ops.render.render(animation=True)
