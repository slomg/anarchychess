import { render, screen } from "@testing-library/react";

import VoteView from "@/features/vote/components/VoteView";
import HomeVote from "../HomeVote";

vi.mock("@/features/vote/components/VoteView");

describe("HomeVote", () => {
    it("should render the heading and subheading", () => {
        render(<HomeVote />);

        expect(screen.getByTestId("homeVote")).toHaveAttribute("id", "vote");
        expect(
            screen.getByRole("heading", { name: "Would You Rather" }),
        ).toBeInTheDocument();
        expect(
            screen.getByText(
                "Ideas from the Discord. Vote for your favorites.",
            ),
        ).toBeInTheDocument();
    });

    it("should render VoteView", () => {
        render(<HomeVote />);

        expect(VoteView).toHaveBeenCalledWith({}, undefined);
    });
});
