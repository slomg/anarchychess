import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChallengeStore,
    createChallengeStore,
} from "@/features/challenges/stores/challengeStore";

import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import ChallengeStoreContext from "@/features/challenges/contexts/challengeContext";
import ChallengeStatusText from "../ChallengeStatusText";
import { ChallengeRequest } from "@/lib/apiClient";

describe("ChallengeStatusText", () => {
    let challengeMock: ChallengeRequest;
    let challengeStore: StoreApi<ChallengeStore>;

    beforeEach(() => {
        challengeMock = createFakeChallengeRequest();
        challengeStore = createChallengeStore({ challenge: challengeMock });
    });

    const renderWithProviders = () =>
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <ChallengeStatusText
                    activeText="Active Challenge"
                    activeClassName="text-active"
                    overClassName="text-over"
                />
            </ChallengeStoreContext.Provider>,
        );

    it("should render active text when not cancelled or expired", () => {
        renderWithProviders();

        const text = screen.getByText("Active Challenge");
        expect(text).toBeInTheDocument();
        expect(text).toHaveClass("text-active");
    });

    it("should render 'Challenge Expired' when expired", () => {
        challengeMock.expiresAt = new Date(
            new Date().getTime() - 100,
        ).toISOString();
        challengeStore.setState({ challenge: challengeMock });
        renderWithProviders();

        const text = screen.getByText("Challenge Expired");
        expect(text).toBeInTheDocument();
        expect(text).toHaveClass("text-over");
    });

    it("should render 'Challenge Cancelled' when cancelled requester", () => {
        challengeMock.cancelledBy = challengeMock.requester.userId;
        renderWithProviders();

        const text = screen.getByText("Challenge Cancelled");
        expect(text).toBeInTheDocument();
        expect(text).toHaveClass("text-over");
    });

    it("should render 'Challenge Declined' when cancelled by recipient", () => {
        challengeMock.cancelledBy = challengeMock.recipient?.userId;
        renderWithProviders();

        const text = screen.getByText("Challenge Declined");
        expect(text).toBeInTheDocument();
        expect(text).toHaveClass("text-over");
    });
});
