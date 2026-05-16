import { render, screen } from "@testing-library/react";

import VoteView from "@/features/vote/components/VoteView";
import HomeVote from "../HomeVote";

vi.mock("@/features/vote/components/VoteView");

describe("HomeVote", () => {
    it("should render the heading", () => {
        render(<HomeVote />);
        expect(
            screen.getByRole("heading", { name: /would you rather/i }),
        ).toBeInTheDocument();
    });

    it("should render VoteView", () => {
        render(<HomeVote />);

        expect(VoteView).toHaveBeenCalledWith({}, undefined);
    });
});
