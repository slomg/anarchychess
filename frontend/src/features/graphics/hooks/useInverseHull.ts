import {
    MeshBasicMaterial,
    Object3DEventMap,
    BackSide,
    Color,
    Group,
    Mesh,
} from "three";

export default function useInverseHull(
    scene: Group<Object3DEventMap>,
    { color, thickness }: { color?: Color; thickness?: number } = {},
): Group<Object3DEventMap> {
    color ??= new Color("black");
    thickness ??= 0.1;

    const hull = scene.clone();
    hull.traverse((child) => {
        const mesh = child as Mesh;

        if (!mesh.isMesh) {
            return;
        }

        mesh.material = new MeshBasicMaterial({
            color,
            side: BackSide,
        });

        const geometry = mesh.geometry.clone();
        const position = geometry.attributes.position;
        const normal = geometry.attributes.normal;

        for (let i = 0; i < position.count; i++) {
            position.setXYZ(
                i,
                position.getX(i) + normal.getX(i) * thickness,
                position.getY(i) + normal.getY(i) * thickness,
                position.getZ(i) + normal.getZ(i) * thickness,
            );
        }
        mesh.geometry = geometry;
    });

    return hull;
}
