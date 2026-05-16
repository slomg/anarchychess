import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { act } from "react";

import {
    completeVote,
    ErrorCode,
    getNextVotePair,
    PendingUserVote,
} from "@/lib/apiClient";

import { createFakePendingUserVote } from "@/lib/testUtils/fakers/pendingUserVoteFaker";
import VoteView, { VOTE_DELAY_MS } from "../VoteView";

vi.mock("@/lib/apiClient/definition");

describe("VoteView", () => {
    const getNextVotePairMock = vi.mocked(getNextVotePair);
    const completeVoteMock = vi.mocked(completeVote);

    let pendingVote: PendingUserVote;

    beforeEach(() => {
        pendingVote = createFakePendingUserVote();

        getNextVotePairMock.mockResolvedValue({
            error: undefined,
            data: pendingVote,
            response: new Response(),
        });
        completeVoteMock.mockResolvedValue({
            error: undefined,
            data: undefined,
            response: new Response(),
        });

        vi.useFakeTimers({ shouldAdvanceTime: true });
    });

    async function fireHoldEnd() {
        await act(() =>
            screen.getByTestId("voteViewHoldProgress").dispatchEvent(
                new TransitionEvent("transitionend", {
                    propertyName: "width",
                    bubbles: true,
                }),
            ),
        );
        await flushMicrotasks();
    }

    it("should render the nothing left message when there are no pairs", async () => {
        getNextVotePairMock.mockResolvedValue({
            error: {
                status: 404,
                errors: [
                    {
                        errorCode: ErrorCode.VOTE_NO_UN_SEEN_PAIR_FOUND,
                        description: "",
                        metadata: {},
                    },
                ],
            },
            data: undefined,
            response: new Response(null, { status: 404 }),
        });

        render(<VoteView />);
        await flushMicrotasks();

        expect(
            screen.getByText(/nothing left for you to vote/i),
        ).toBeInTheDocument();
    });

    it("should mark option A as selected when its card is clicked", async () => {
        const user = userEvent.setup();
        render(<VoteView />);
        await flushMicrotasks();

        await user.click(screen.getByText(pendingVote.optionA.name));

        expect(screen.getByTestId("voteCard-A")).toHaveClass(
            "outline-secondary",
        );
    });

    it("should switch selection when a different card is clicked", async () => {
        const user = userEvent.setup();
        render(<VoteView />);
        await flushMicrotasks();

        await user.click(screen.getByText(pendingVote.optionA.name));
        await user.click(screen.getByText(pendingVote.optionB.name));

        expect(screen.getByTestId("voteCard-B")).toHaveClass(
            "outline-secondary",
        );
        expect(screen.getByTestId("voteCard-A")).not.toHaveClass(
            "outline-secondary",
        );
    });

    it("should show the selected option's name in the button", async () => {
        const user = userEvent.setup();
        render(<VoteView />);
        await flushMicrotasks();

        await user.click(screen.getByText(pendingVote.optionB.name));

        expect(screen.getByRole("button")).toHaveTextContent(
            `LONG PRESS TO CAST VOTE FOR ` +
                pendingVote.optionB.name.toUpperCase(),
        );
    });

    it("should cast vote after holding the button for 4 seconds", async () => {
        const user = userEvent.setup();
        render(<VoteView />);
        await flushMicrotasks();

        await user.click(screen.getByText(pendingVote.optionA.name));
        await user.pointer({
            keys: "[MouseLeft>]",
            target: screen.getByRole("button"),
        });
        await fireHoldEnd();

        expect(completeVoteMock).toHaveBeenCalledExactlyOnceWith({
            query: { optionKey: pendingVote.optionA.optionKey },
        });
    });

    it("should load the next pair after a successful vote", async () => {
        const nextVote = createFakePendingUserVote();
        getNextVotePairMock
            .mockResolvedValueOnce({
                error: undefined,
                data: pendingVote,
                response: new Response(),
            })
            .mockResolvedValueOnce({
                error: undefined,
                data: nextVote,
                response: new Response(),
            });

        const user = userEvent.setup();
        render(<VoteView />);
        await flushMicrotasks();

        await user.click(screen.getByText(pendingVote.optionB.name));
        await user.pointer({
            keys: "[MouseLeft>]",
            target: screen.getByRole("button"),
        });
        await fireHoldEnd();
        await act(() => vi.advanceTimersByTimeAsync(VOTE_DELAY_MS));

        expect(completeVoteMock).toHaveBeenCalledExactlyOnceWith({
            query: { optionKey: pendingVote.optionB.optionKey },
        });

        expect(screen.getByText(nextVote.optionA.name)).toBeInTheDocument();
    });

    it("should show a rate limit error on a 429 response", async () => {
        completeVoteMock.mockResolvedValue({
            error: undefined,
            data: undefined,
            response: new Response(null, { status: 429 }),
        });

        const user = userEvent.setup();
        render(<VoteView />);
        await flushMicrotasks();

        await user.click(screen.getByText(pendingVote.optionA.name));
        await user.pointer({
            keys: "[MouseLeft>]",
            target: screen.getByRole("button"),
        });
        await fireHoldEnd();
        await act(() => vi.advanceTimersByTimeAsync(VOTE_DELAY_MS));

        expect(
            screen.getByText(
                "You are being rate limited. Please try again later.",
            ),
        ).toBeInTheDocument();
    });

    it("should show a generic error when completeVote fails", async () => {
        completeVoteMock.mockResolvedValue({
            error: { errors: [] },
            data: undefined,
            response: new Response(),
        });

        const user = userEvent.setup();
        render(<VoteView />);
        await flushMicrotasks();

        await user.click(screen.getByText(pendingVote.optionA.name));
        const button = screen.getByRole("button");
        await user.pointer({ keys: "[MouseLeft>]", target: button });
        await fireHoldEnd();
        await act(() => vi.advanceTimersByTimeAsync(VOTE_DELAY_MS));

        expect(screen.getByText("Something went wrong.")).toBeInTheDocument();
    });

    it("should not finish voting if the vote delay has not elapsed", async () => {
        const user = userEvent.setup();
        render(<VoteView />);
        await flushMicrotasks();

        await user.click(screen.getByText(pendingVote.optionA.name));
        await user.pointer({
            keys: "[MouseLeft>]",
            target: screen.getByRole("button"),
        });
        await fireHoldEnd();

        expect(
            screen.queryByText(pendingVote.optionB.name),
        ).toBeInTheDocument();
    });
});
