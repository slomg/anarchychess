import { VoteOption } from "@/lib/apiClient";
import { createFakeVoteOption } from "@/lib/testUtils/fakers/voteOptionFaker";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import VoteCard from "../VoteCard";

describe("VoteCard", () => {
    let option: VoteOption;

    beforeEach(() => {
        option = createFakeVoteOption();
    });

    it("should render the option name", () => {
        render(
            <VoteCard
                option={option}
                optionLetter="A"
                isSelected={false}
                onClick={vi.fn()}
            />,
        );
        expect(screen.getByText(option.name)).toBeInTheDocument();
    });

    it("should render the option description", () => {
        render(
            <VoteCard
                option={option}
                optionLetter="A"
                isSelected={false}
                onClick={vi.fn()}
            />,
        );
        expect(screen.getByText(option.description)).toBeInTheDocument();
    });

    it("should render the option letter in the badge and label", () => {
        render(
            <VoteCard
                option={option}
                optionLetter="B"
                isSelected={false}
                onClick={vi.fn()}
            />,
        );

        expect(screen.getByText("B")).toBeInTheDocument();
        expect(screen.getByText("OPTION B")).toBeInTheDocument();
    });

    it("should call onClick when clicked", async () => {
        const handleClick = vi.fn();

        const user = userEvent.setup();
        render(
            <VoteCard
                option={option}
                optionLetter="A"
                isSelected={false}
                onClick={handleClick}
            />,
        );

        await user.click(screen.getByRole("article"));
        expect(handleClick).toHaveBeenCalledOnce();
    });

    it("should apply the selected outline class when isSelected is true", () => {
        render(
            <VoteCard
                option={option}
                optionLetter="A"
                isSelected={true}
                onClick={vi.fn()}
            />,
        );
        expect(screen.getByRole("article")).toHaveClass("outline-secondary");
    });

    it("should not apply the selected outline class when isSelected is false", () => {
        render(
            <VoteCard
                option={option}
                optionLetter="A"
                isSelected={false}
                onClick={vi.fn()}
            />,
        );
        expect(screen.getByRole("article")).not.toHaveClass(
            "outline-secondary",
        );
    });
});
