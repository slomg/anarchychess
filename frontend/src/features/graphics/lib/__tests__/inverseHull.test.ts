import {
    Group,
    Mesh,
    BoxGeometry,
    MeshBasicMaterial,
    BackSide,
    Color,
} from "three";

import inverseHull from "../inverseHull";

describe("inverseHull", () => {
    let scene: Group;
    let originalPositions: number[][] = [];
    let originalNormals: number[][] = [];

    beforeEach(() => {
        scene = new Group();
        const geometry = new BoxGeometry(1, 1, 1);
        const mesh = new Mesh(
            geometry,
            new MeshBasicMaterial({ color: "white" }),
        );
        scene.add(mesh);

        const pos = geometry.attributes.position;
        const norm = geometry.attributes.normal;

        originalPositions = [];
        originalNormals = [];
        for (let i = 0; i < pos.count; i++) {
            originalPositions.push([pos.getX(i), pos.getY(i), pos.getZ(i)]);
            originalNormals.push([norm.getX(i), norm.getY(i), norm.getZ(i)]);
        }
    });

    it("should return a cloned scene", () => {
        const hull = inverseHull(scene);
        expect(hull).not.toBe(scene);
        expect(hull.children.length).toBe(scene.children.length);
    });

    it("should set mesh materials to provided color", () => {
        const color = new Color("red");
        const hull = inverseHull(scene, { color });

        const mesh = hull.children[0] as Mesh;
        expect(mesh.material).toBeInstanceOf(MeshBasicMaterial);
        expect((mesh.material as MeshBasicMaterial).color).toEqual(color);
    });

    it("should set mesh materials to back side", () => {
        const hull = inverseHull(scene);

        const mesh = hull.children[0] as Mesh;
        expect(mesh.material).toBeInstanceOf(MeshBasicMaterial);
        expect((mesh.material as MeshBasicMaterial).side).toBe(BackSide);
    });

    it("should offset geometry vertices by normal * thickness", () => {
        const thickness = 0.2;
        const hull = inverseHull(scene, { thickness });
        const newMesh = hull.children[0] as Mesh;
        const newPos = newMesh.geometry.attributes.position;

        for (let i = 0; i < originalPositions.length; i++) {
            const [ox, oy, oz] = originalPositions[i];
            const [nx, ny, nz] = originalNormals[i];
            expect(newPos.getX(i)).toBeCloseTo(ox + nx * thickness);
            expect(newPos.getY(i)).toBeCloseTo(oy + ny * thickness);
            expect(newPos.getZ(i)).toBeCloseTo(oz + nz * thickness);
        }
    });
});
