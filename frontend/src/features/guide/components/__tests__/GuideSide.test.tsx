import { render, screen } from "@testing-library/react";
import GuideSide from "../GuideSide";

describe("GuideSide component", () => {
    it("should set the correct href for the Pieces and Rules links", () => {
        render(
            <GuideSide piecesGuideHref={"#pieces"} rulesGuideHref={"#rules"} />,
        );
        const piecesLink = screen.getByText("Pieces");
        const rulesLink = screen.getByText("Rules");

        expect(piecesLink.getAttribute("href")).toBe("#pieces");
        expect(rulesLink.getAttribute("href")).toBe("#rules");
    });
});
