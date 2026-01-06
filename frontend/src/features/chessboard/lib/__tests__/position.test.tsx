import { createFakePositionProps } from "@/lib/testUtils/fakers/positionPropsFaker";
import {
    ChildPositionNode,
    PositionProps,
    RootPositionNode,
} from "../position";
import BoardPieces from "../boardPieces";
import { createFakeBoardPieces } from "@/lib/testUtils/fakers/chessboardFakers";

describe("RootPositionNode", () => {
    let root: RootPositionNode;
    let pieces: BoardPieces;

    beforeEach(() => {
        pieces = createFakeBoardPieces();
        root = new RootPositionNode(pieces);
    });

    describe("constructor", () => {
        it("should create a root node with correct pieces", () => {
            expect(root.pieces).toBe(pieces);
            expect(root.next).toBeNull();
            expect(root.variations).toEqual([]);
            expect(root.subVariationByKey.size).toBe(0);
        });
    });

    describe("isPositionNext", () => {
        it("should return false if given position is null", () => {
            expect(root.isPositionNext(null)).toBe(false);
        });

        it("should return true if given position is the main variation", () => {
            const { child: mainChild } = root.createChild(
                createFakePositionProps(),
            );

            expect(root.isPositionNext(mainChild)).toBe(true);
        });

        it("should return true if given position is a sub variation", () => {
            root.createChild(createFakePositionProps());
            const { child: subChild } = root.createChild(
                createFakePositionProps(),
            );

            expect(root.isPositionNext(subChild)).toBe(true);
        });

        it("should return false if given position is neither main nor sub variation", () => {
            root.createChild(createFakePositionProps());
            const otherNode = new ChildPositionNode(createFakePositionProps());

            expect(root.isPositionNext(otherNode)).toBe(false);
        });
    });

    describe("createChild", () => {
        it("should create a main child variation correctly", () => {
            const childProps = createFakePositionProps();
            const { child, isMainVariation } = root.createChild(childProps);

            expect(isMainVariation).toBe(true);
            expect(root.next).toBe(child);
            expect(root.variations).toContain(child);
            expect(root.subVariationByKey.size).toBe(0);
            expect(child.prev).toBeNull();
            expect(child.ply).toBe(0);
            expect(child).toEqual(expect.objectContaining(childProps));
        });

        it("should return the main variation if move key matches", () => {
            const props = createFakePositionProps();
            const { child: firstChild } = root.createChild(props);
            const { child: duplicateChild, isMainVariation } =
                root.createChild(props);

            expect(duplicateChild).toBe(firstChild);
            expect(isMainVariation).toBe(true);
            expect(root.variations).toHaveLength(1);
        });

        it("should create sub variations correctly", () => {
            const subProps = createFakePositionProps();

            const { child: mainChild } = root.createChild(
                createFakePositionProps(),
            );
            const { child: subChild, isMainVariation } =
                root.createChild(subProps);

            expect(subChild.prev).toBeNull();
            expect(isMainVariation).toBe(false);
            expect(root.variations).toHaveLength(2);
            expect(root.subVariationByKey.get(subProps.move.moveKey)).toBe(
                subChild,
            );
            expect(root.next).toBe(mainChild);
        });

        it("should set isMainVariation false for a duplicate sub variation", () => {
            const subProps = createFakePositionProps();

            root.createChild(createFakePositionProps());
            const { child: subChild } = root.createChild(subProps);
            const { child: duplicateSub, isMainVariation } =
                root.createChild(subProps);

            expect(duplicateSub).toBe(subChild);
            expect(isMainVariation).toBe(false);
            expect(root.subVariationByKey.get(subProps.move.moveKey)).toBe(
                subChild,
            );
            expect(root.variations).toHaveLength(2);
        });
    });

    describe("iterator", () => {
        it("should iterate over mainline positions correctly", () => {
            const { child } = root.createChild(createFakePositionProps());
            expect([...root]).toEqual([child]);
        });
    });
});

describe("ChildPositionNode", () => {
    let rootProps: PositionProps;
    let child: ChildPositionNode;

    beforeEach(() => {
        rootProps = createFakePositionProps();
        child = new ChildPositionNode(rootProps);
    });

    describe("constructor", () => {
        it("should create a child node with correct properties", () => {
            expect(child.pieces).toEqual(rootProps.pieces);
            expect(child.fen).toBe(rootProps.fen);
            expect(child.sideToMove).toBe(rootProps.sideToMove);
            expect(child.move).toBe(rootProps.move);
            expect(child.san).toBe(rootProps.san);
            expect(child.ply).toBe(0);
            expect(child.prev).toBeNull();
            expect(child.next).toBeNull();
            expect(child.variations).toEqual([]);
            expect(child.subVariationByKey.size).toBe(0);
        });
    });

    describe("createChild", () => {
        it("should create a main child variation correctly", () => {
            const props = createFakePositionProps();
            const { child: created, isMainVariation } =
                child.createChild(props);

            expect(isMainVariation).toBe(true);
            expect(child.next).toBe(created);
            expect(child.variations).toEqual([created]);
            expect(child.subVariationByKey.size).toBe(0);
            expect(created.prev).toBe(child);
            expect(created.ply).toBe(child.ply + 1);
            expect(created).toEqual(expect.objectContaining(props));
        });

        it("should create sub variation correctly", () => {
            const props = createFakePositionProps();

            child.createChild(createFakePositionProps());
            const { child: createdSub, isMainVariation } =
                child.createChild(props);

            expect(isMainVariation).toBe(false);
            expect(createdSub.prev).toBe(child);
            expect(createdSub.ply).toBe(child.ply + 1);
            expect(createdSub).toEqual(expect.objectContaining(props));
        });
    });

    describe("iterator", () => {
        it("should iterate over mainline positions correctly", () => {
            const { child: mainChild } = child.createChild(
                createFakePositionProps(),
            );
            expect([...child]).toEqual([child, mainChild]);
        });
    });
});
