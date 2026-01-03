import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import { PositionNode, PositionProps } from "../position";

describe("PositionNode", () => {
    let root: PositionNode;
    let props: PositionProps;

    beforeEach(() => {
        props = createFakePositionProps();
        root = new PositionNode(props);
    });

    it("should create a root position with correct properties", () => {
        expect(root.pieces).toEqual(props.pieces);
        expect(root.fen).toBe(props.fen);
        expect(root.sideToMove).toBe(props.sideToMove);
        expect(root.move).toBe(props.move);
        expect(root.san).toBe(props.san);
        expect(root.ply).toBe(0);
        expect(root.prev).toBeNull();
        expect(root.next).toBeNull();
        expect(root.variations).toEqual([]);
        expect(root.subVariationBySan.size).toBe(0);
        expect(root.positionId).toBeTypeOf("string");
    });

    it("should create a main child variation correctly", () => {
        const child = root.createChild(createFakePositionProps());

        expect(child.prev).toBe(root);
        expect(child.ply).toBe(1);
        expect(root.next).toBe(child);
        expect(root.variations).toContain(child);
        expect(root.subVariationBySan.size).toBe(0);
    });

    it("should return the main variation if SAN matches", () => {
        const props = createFakePositionProps();
        const firstChild = root.createChild(props);
        const duplicateChild = root.createChild(props);

        expect(duplicateChild).toBe(firstChild);
        expect(root.variations.length).toBe(1);
    });

    it("should create sub-variations correctly", () => {
        const mainChildProps = createFakePositionProps({ san: "e5" });
        const subChildProps = createFakePositionProps({ san: "c5" });

        const mainChild = root.createChild(mainChildProps);
        const subChild = root.createChild(subChildProps);

        expect(subChild.prev).toBe(root);
        expect(root.variations).toHaveLength(2);
        expect(root.subVariationBySan.get(subChildProps.san)).toBe(subChild);
        expect(root.next).toBe(mainChild);
    });

    it("should iterate over positions correctly", () => {
        const child = root.createChild(createFakePositionProps());
        expect([...root]).toEqual([root, child]);
    });
});
