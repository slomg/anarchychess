import {
    Color,
    Group,
    Material,
    Mesh,
    MeshStandardMaterial,
    Object3DEventMap,
} from "three";

export default function replaceSceneColor(
    scene: Group<Object3DEventMap>,
    from: Color,
    to: Color,
): Group<Object3DEventMap> {
    const colored = scene.clone();
    colored.traverse((child) => {
        if (!(child instanceof Mesh)) {
            return;
        }

        if (Array.isArray(child.material)) {
            child.material = child.material.map((material) =>
                replaceMaterialColor(material, from, to),
            );
        } else {
            child.material = replaceMaterialColor(child.material, from, to);
        }
    });
    return colored;
}

function replaceMaterialColor(mat: Material, from: Color, to: Color): Material {
    if (!hasColor(mat) || !mat.color.equals(from)) {
        return mat;
    }

    const clone = mat.clone();
    clone.color.set(to);
    return clone;
}

function hasColor(mat: Material): mat is MeshStandardMaterial {
    return "color" in mat;
}
