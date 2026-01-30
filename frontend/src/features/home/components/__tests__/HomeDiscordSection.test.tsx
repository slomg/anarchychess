import { render, screen } from "@testing-library/react";

import HomeDiscordSection from "../HomeDiscordSection";
import constants from "@/lib/constants";

describe("HomeDiscordSection", () => {
    it("should render the section heading", () => {
        render(<HomeDiscordSection />);

        expect(
            screen.getByRole("heading", { name: /want something added/i }),
        ).toBeInTheDocument();
    });

    it("should render both discord message previews", () => {
        render(<HomeDiscordSection />);

        expect(screen.getByText("Sniper Bishop")).toBeInTheDocument();
        expect(screen.getByText("slomg")).toBeInTheDocument();

        expect(screen.getByText(/overtime chaos/i)).toBeInTheDocument();

        expect(screen.getByText(/omg i love this/i)).toBeInTheDocument();
    });

    it("should render the join discord call to action", () => {
        render(<HomeDiscordSection />);

        expect(
            screen.getByRole("button", { name: /join discord/i }),
        ).toBeInTheDocument();
    });

    it("should link to the discord invite page", () => {
        render(<HomeDiscordSection />);

        const link = screen.getByRole("link", { name: /join discord/i });

        expect(link).toHaveAttribute("href", constants.PATHS.DISCORD);
    });

    it("should render all images with accessible alt text", () => {
        render(<HomeDiscordSection />);

        expect(screen.getByAltText(/sniper bishop pfp/i)).toBeInTheDocument();
        expect(screen.getByAltText(/slomg pfp/i)).toBeInTheDocument();
        expect(screen.getByAltText(/discord logo/i)).toBeInTheDocument();
        expect(screen.getByAltText(/pog/i)).toBeInTheDocument();
    });
});
