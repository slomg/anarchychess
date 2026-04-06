import { Color, Group, Material, Mesh, Object3DEventMap } from "three";

export default function replaceSceneColor(
    scene: Group<Object3DEventMap>,
    from: Color,
    to: Color,
): Group<Object3DEventMap> {
    const colored = scene.clone();
    colored.traverse((child) => {
        const mesh = child as Mesh;
        if (!mesh.isMesh) {
            return;
        }

        const mat = mesh.material as Material & { color?: Color };
        if (mat.color && mat.color.equals(from)) {
            mat.color.set(to);
        }
    });
    return colored;
}
