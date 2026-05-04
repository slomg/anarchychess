import { cubicBezier } from "../mathUtils";

describe("cubicBezier", () => {
    it("should return p0 when t = 0", () => {
        expect(cubicBezier(0, 10, 20, 30, 40)).toBe(10);
    });

    it("should return p3 when t = 1", () => {
        expect(cubicBezier(1, 10, 20, 30, 40)).toBe(40);
    });

    it("should interpolate midpoint symmetrically at t = 0.5", () => {
        const result = cubicBezier(0.5, 0, 0, 0, 10);
        expect(result).toBeCloseTo(1.25);
    });

    it("should work for linear case (all points equal slope)", () => {
        const result = cubicBezier(0.3, 5, 5, 5, 5);
        expect(result).toBeCloseTo(5);
    });

    it("should behave like linear interpolation when control points are collinear", () => {
        const p0 = 0,
            p1 = 10,
            p2 = 20,
            p3 = 30;
        const t = 0.25;

        const result = cubicBezier(t, p0, p1, p2, p3);
        const expected = 7.5;

        expect(result).toBeCloseTo(expected);
    });
});
