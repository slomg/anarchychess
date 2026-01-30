import { StoreApi } from "zustand";
import {
    ChallengeStore,
    createChallengeStore,
} from "../../stores/challengeStore";
import { ChallengeRequest } from "@/lib/apiClient";
import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import {
    ChallengeClientEvents,
    useChallengeInstanceEvent,
} from "../useChallengeHub";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";
import { act, renderHook } from "@testing-library/react";
import useChallengeEvents from "../useChallengeEvents";
import constants from "@/lib/constants";

vi.mock("@/features/challenges/hooks/useChallengeHub");

describe("useChallengeEvents", () => {
    let challengeStore: StoreApi<ChallengeStore>;
    let challengeMock: ChallengeRequest;

    const useChallengeInstanceEventMock = vi.mocked(useChallengeInstanceEvent);
    const challengeEventHandlers: EventHandlers<ChallengeClientEvents> = {};

    beforeEach(() => {
        challengeMock = createFakeChallengeRequest();
        challengeStore = createChallengeStore({ challenge: challengeMock });

        useChallengeInstanceEventMock.mockImplementation(
            (challengeToken, event, handler) => {
                if (challengeToken === challengeMock.challengeToken)
                    challengeEventHandlers[event] = handler;
            },
        );
    });

    describe("ChallengeAcceptedAsync", () => {
        it("should redirect if the challenge token is correct", async () => {
            const routerMock = mockRouter();
            const gameToken = "test game token";
            renderHook(() =>
                useChallengeEvents(
                    challengeStore,
                    challengeMock.challengeToken,
                ),
            );

            await act(() =>
                challengeEventHandlers.ChallengeAcceptedAsync?.(
                    gameToken,
                    challengeMock.challengeToken,
                ),
            );

            expect(routerMock.push).toHaveBeenCalledExactlyOnceWith(
                `${constants.PATHS.GAME}/${gameToken}`,
            );
        });

        it("should not redirect if the challenge token is incorrect", async () => {
            const routerMock = mockRouter();
            renderHook(() =>
                useChallengeEvents(
                    challengeStore,
                    challengeMock.challengeToken,
                ),
            );

            await act(() =>
                challengeEventHandlers.ChallengeAcceptedAsync?.(
                    "test game token",
                    "some random challenge",
                ),
            );

            expect(routerMock.push).not.toHaveBeenCalled();
        });
    });

    describe("ChallengeCancelledAsync", () => {
        it("should mark as cancelled if the challenge token is correct", async () => {
            const cancelledBy = "cancelled by";
            renderHook(() =>
                useChallengeEvents(
                    challengeStore,
                    challengeMock.challengeToken,
                ),
            );

            await act(() =>
                challengeEventHandlers.ChallengeCancelledAsync?.(
                    cancelledBy,
                    challengeMock.challengeToken,
                ),
            );

            expect(challengeStore.getState().challenge.cancelledBy).toBe(
                cancelledBy,
            );
        });

        it("should not do anything if the challenge token is incorrect", async () => {
            renderHook(() =>
                useChallengeEvents(
                    challengeStore,
                    challengeMock.challengeToken,
                ),
            );

            await act(() =>
                challengeEventHandlers.ChallengeCancelledAsync?.(
                    "cancelled by",
                    "some random challenge",
                ),
            );

            expect(
                challengeStore.getState().challenge.cancelledBy,
            ).toBeNullable();
        });
    });

    describe("ReceiveUpdatedChallengeAsync", () => {
        it("should set the challenge if the challenge token is correct", async () => {
            const routerMock = mockRouter();
            const newChallenge = createFakeChallengeRequest({
                challengeToken: challengeMock.challengeToken,
            });
            renderHook(() =>
                useChallengeEvents(
                    challengeStore,
                    challengeMock.challengeToken,
                ),
            );

            await act(() =>
                challengeEventHandlers.ReceiveUpdatedChallengeAsync?.(
                    newChallenge,
                ),
            );

            const setChallenge = challengeStore.getState().challenge;
            expect(setChallenge).toEqual(newChallenge);
            expect(setChallenge).not.toEqual(challengeMock);
            expect(routerMock.push).not.toHaveBeenCalled();
        });

        it("should redirect if resolvedGame is defined", async () => {
            const routerMock = mockRouter();
            const newChallenge = createFakeChallengeRequest({
                challengeToken: challengeMock.challengeToken,
                resolvedGame: "game token",
            });
            renderHook(() =>
                useChallengeEvents(
                    challengeStore,
                    challengeMock.challengeToken,
                ),
            );

            await act(() =>
                challengeEventHandlers.ReceiveUpdatedChallengeAsync?.(
                    newChallenge,
                ),
            );

            expect(routerMock.push).toHaveBeenCalledExactlyOnceWith(
                `${constants.PATHS.GAME}/${newChallenge.resolvedGame}`,
            );
        });

        it("should do nothing if the challenge token is incorrect", async () => {
            const routerMock = mockRouter();
            const newChallenge = createFakeChallengeRequest({
                challengeToken: "different token",
            });
            renderHook(() =>
                useChallengeEvents(
                    challengeStore,
                    challengeMock.challengeToken,
                ),
            );

            await act(() =>
                challengeEventHandlers.ReceiveUpdatedChallengeAsync?.(
                    newChallenge,
                ),
            );

            expect(challengeStore.getState().challenge).toEqual(challengeMock);
            expect(routerMock.push).not.toHaveBeenCalled();
        });
    });
});
