import { describe, it, expect, beforeEach } from "vitest";
import { Group, Mesh, MeshBasicMaterial, Color, BoxGeometry } from "three";
import replaceSceneColor from "../replaceSceneColor";

describe("replaceSceneColor", () => {
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

    it("should replace the color exactly from 'from' to 'to' in the cloned scene", () => {
        const from = new Color("white");
        const to = new Color("red");

        const result = replaceSceneColor(scene, from, to);

        const [resultWhiteMesh, resultBlackMesh] = result.children as Mesh[];

        expect((resultWhiteMesh.material as MeshBasicMaterial).color).toEqual(
            to,
        );
        expect((resultBlackMesh.material as MeshBasicMaterial).color).toEqual(
            black,
        );

        // original scene should remain unchanged
        expect((whiteMesh.material as MeshBasicMaterial).color).toEqual(white);
        expect((blackMesh.material as MeshBasicMaterial).color).toEqual(black);
    });

    it("should ignore non-mesh children", () => {
        const from = new Color("white");
        const to = new Color("blue");

        const result = replaceSceneColor(scene, from, to);

        const group = result.children[2];
        expect(group.type).toBe("Group");
    });
});
