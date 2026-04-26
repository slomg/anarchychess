import { act, renderHook } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChallengeStore,
    createChallengeStore,
} from "../../stores/challengeStore";
import {
    ChallengeClientEvents,
    useChallengeInstanceEvent,
} from "../useChallengeHub";

import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import gameStartRedirect from "@/features/liveGame/lib/gameStartRedirect";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import useChallengeEvents from "../useChallengeEvents";
import { ChallengeRequest } from "@/lib/apiClient";

vi.mock("@/features/challenges/hooks/useChallengeHub");
vi.mock("@/features/liveGame/lib/gameStartRedirect");

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

            expect(gameStartRedirect).toHaveBeenCalledExactlyOnceWith(
                gameToken,
                expect.anything(),
            );
        });

        it("should not redirect if the challenge token is incorrect", async () => {
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

            expect(gameStartRedirect).not.toHaveBeenCalled();
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
            expect(gameStartRedirect).not.toHaveBeenCalled();
        });

        it("should redirect if resolvedGame is defined", async () => {
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

            expect(gameStartRedirect).toHaveBeenCalledExactlyOnceWith(
                newChallenge.resolvedGame,
                expect.anything(),
            );
        });

        it("should do nothing if the challenge token is incorrect", async () => {
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
            expect(gameStartRedirect).not.toHaveBeenCalled();
        });
    });
});
