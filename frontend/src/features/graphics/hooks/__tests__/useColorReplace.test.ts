import { describe, it, expect, beforeEach } from "vitest";
import { Group, Mesh, MeshBasicMaterial, Color, BoxGeometry } from "three";
import { useColorReplace } from "../useColorReplace";

describe("useColorReplace", () => {
    let scene: Group;
    let whiteMesh: Mesh;
    let blackMesh: Mesh;

    const white = new Color("white");
    const black = new Color("black");

    beforeEach(() => {
        scene = new Group();

        whiteMesh = new Mesh(
            new BoxGeometry(1, 1, 1),
            new MeshBasicMaterial({ color: white }),
        );
        scene.add(whiteMesh);

        blackMesh = new Mesh(
            new BoxGeometry(1, 1, 1),
            new MeshBasicMaterial({ color: black }),
        );
        scene.add(blackMesh);

        scene.add(new Group());
    });

    it("should replace the color exactly from 'from' to 'to'", () => {
        const from = new Color("white");
        const to = new Color("red");

        useColorReplace(scene, from, to);

        expect((whiteMesh.material as MeshBasicMaterial).color).toEqual(to);
        expect((blackMesh.material as MeshBasicMaterial).color).toEqual(black);
    });

    it("should ignore non-mesh children", () => {
        const from = new Color("white");
        const to = new Color("blue");

        useColorReplace(scene, from, to);

        const group = scene.children[2];
        expect(group.type).toBe("Group");
    });
});
