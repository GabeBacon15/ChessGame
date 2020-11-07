bl_info = {
    "name" : "Three Player Chess Anime Generator",
    "author" : "Gabriel Bacon",
    "version" : (1,0),
    "blender" : (2,83,5),
    "location" : "View3d > Tool",
    "description" : "This only applies to a very specific chess board for a game i am working on. If you see this it means I put it online for fun. It will take in a sequence of moves a piece will make on a 3 player chess board and generates and sets the keyframes to transition from space to space.",
    "warning" : "I am Bad at this",
    "wiki_url" : "",
    "category" : "3D Chess Board?",
}

import bpy
import mathutils

class TestPanel(bpy.types.Panel):
    bl_label = "Piece Keyframe Gen"
    bl_idname = "P_Keyframe_Gen"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'Anim Gen'
    
    def draw(self, context):
        layout = self.layout
        
        row = layout.row()
        
        row.operator('gananim.operator')
        
        
class GENERATE_ANIM(bpy.types.Operator):
    bl_label = "Generate Animations"
    bl_idname = 'gananim.operator'
    
    def execute(self, context):
        
        object = bpy.context.active_object
        cf = context.scene.frame_current
        keyInterp = context.preferences.edit.keyframe_new_interpolation_type
        context.preferences.edit.keyframe_new_interpolation_type ='LINEAR'
        
        object.keyframe_insert(data_path='location', frame=(cf))
        object.keyframe_insert(data_path='rotation_euler', frame=(cf))
        
        # one blender unit in x-direction
        vec = mathutils.Vector((0.0, 0.0, -2.0))
        inv = object.matrix_world.copy()
        inv.invert()
        # vec aligned to local axis in Blender 2.8+
        # in previous versions: vec_rot = vec * inv
        vec_rot = vec @ inv
        object.location = object.location + vec_rot

        return{'FINISHED'}
        
        
def register():
    bpy.utils.register_class(TestPanel)
    bpy.utils.register_class(GENERATE_ANIM)
    
def unregister():
    bpy.utils.unregister_class(TestPanel)
    bpy.utils.unregister_class(GENERATE_ANIM)
    
if(__name__ == "__main__"):
    register()