extends Node

func load_model(path: String) -> Node3D:
	return _load_model(path)

func _load_model(path: String) -> Node3D:
	var gltf: GLTFDocument = GLTFDocument.new()
	var vrm_extension: GLTFDocumentExtension = preload(
        "res://addons/vrm/vrm_extension.gd"
	).new()

	gltf.register_gltf_document_extension(vrm_extension, true)

	var state: GLTFState = GLTFState.new()

	var err = gltf.append_from_file(path, state, 8)

	if err != OK:
		gltf.unregister_gltf_document_extension(vrm_extension)
		return null

	var generated_scene = gltf.generate_scene(state)

	gltf.unregister_gltf_document_extension(vrm_extension)

	return generated_scene
