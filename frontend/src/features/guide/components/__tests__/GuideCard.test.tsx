import { render, screen } from "@testing-library/react";
import GuideCard, { GuidePoints } from "../GuideCard";
import Image, { StaticImageData } from "next/image";
import { mock } from "vitest-mock-extended";

vi.mock("next/image");

describe("GuideCard", () => {
    const imagesMock = [
        mock<StaticImageData>({ src: "image1.jpg" }),
        mock<StaticImageData>({ src: "image2.jpg" }),
        mock<StaticImageData>({ src: "image3.jpg" }),
    ];

    const points: GuidePoints = [
        "level 1",
        {
            title: "level 1 parent",
            points: ["level 2 child"],
        },
    ];
    const title = "Test Title";

    it("renders nested list structure correctly", () => {
        render(<GuideCard title={title} points={points} images={imagesMock} />);

        const root = screen.getByTestId("guideCard");

        // first-level list
        const topLevelList = root.querySelector(":scope > div > ul");
        expect(topLevelList).toBeInTheDocument();

        const topItems = topLevelList!.querySelectorAll(":scope > li");
        expect(topItems.length).toBe(2);

        // second item contains nested list
        const nestedContainer = topItems[1];

        const nestedList = nestedContainer.querySelector("ul");
        expect(nestedList).toBeInTheDocument();

        const nestedItems = nestedList!.querySelectorAll("li");
        expect(nestedItems.length).toBe(1);
        expect(nestedItems[0]).toHaveTextContent("level 2 child");
    });

    it("should render the carousel with the correct images and alt text", () => {
        render(<GuideCard title={title} points={points} images={imagesMock} />);

        const carousel = screen.getByTestId("carousel");
        expect(carousel).toBeInTheDocument();

        imagesMock.forEach((_, i) => {
            const img = screen.getByTestId(`carouselImage-${i}`);
            expect(Image).toHaveBeenCalledWith(
                expect.objectContaining({ src: imagesMock[i] }),
                undefined,
            );
            expect(img).toHaveAttribute("alt", `${title} Example ${i + 1}`);
        });
    });

    it("should generate a slugified id from the title", () => {
        render(
            <GuideCard title="some rule" points={points} images={imagesMock} />,
        );

        const card = screen.getByTestId("guideCard");
        expect(card).toHaveAttribute("id", "some-rule");
    });
});
