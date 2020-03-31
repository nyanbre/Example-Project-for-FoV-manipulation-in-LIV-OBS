import obspython as obs
import socket
import sys

server_address = ('localhost', 50734)

def script_load(settings):
    global topLeftPos
    global centerPos
    global originalScale
    global scene_item
    global scene_source

    print("LIV FoV-effects script loading...")

    topLeftPos = obs.vec2()
    centerPos = obs.vec2()
    originalScale = obs.vec2()
    scene_item = None
    scene_source = None

    scene = obs.obs_frontend_get_current_scene()
    if scene is not None:
        scene = obs.obs_scene_from_source(scene)
        items = obs.obs_scene_enum_items(scene)
        for item in items:
            if item is not None:
                source_t = obs.obs_sceneitem_get_source(item)
                if "LIV" in obs.obs_source_get_name(source_t):
                    scene_source = source_t
                    scene_item = item

                    obs.obs_sceneitem_get_pos(scene_item, topLeftPos)
                    obs.obs_sceneitem_get_scale(scene_item, originalScale)
                    print("LIV source is found successfully")

def script_tick(sec):
    global topLeftPos
    global centerPos
    global originalScale
    global scene_item
    global scene_source

    try:
        newFovMultiplier = 0.0
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.connect(server_address)
            s.sendall(b'-')
            data = s.recv(1024)
            newFovMultiplier = float(data)

        # print('new FOV multiplier is: ', newFovMultiplier)

        newScale = obs.vec2()
        newScale.x = originalScale.x * newFovMultiplier
        newScale.y = originalScale.y * newFovMultiplier
        obs.obs_sceneitem_set_scale(scene_item, newScale)
        obs.obs_sceneitem_set_pos(scene_item, newScale)
    except Exception as e:
        pass

def script_update(settings):
    pass

def script_defaults(settings):
    pass
