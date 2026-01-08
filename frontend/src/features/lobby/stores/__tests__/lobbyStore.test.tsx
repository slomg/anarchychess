import { createFakeOngoingGame } from "@/lib/testUtils/fakers/ongoingGameFaker";
import createFakeOpenSeek from "@/lib/testUtils/fakers/openSeekFaker";
import OpenSeekTracker from "../../lib/openSeekTracker";
import { PoolKeyStr } from "../../lib/types";
import useLobbyStore from "../lobbyStore";

describe("lobbyStore", () => {
    beforeEach(() => {
        useLobbyStore.setState(useLobbyStore.getInitialState());
    });

    describe("clearSeeks", () => {
        it("should remove all seeks", () => {
            useLobbyStore.setState({
                seeks: new Set<PoolKeyStr>(["0-5+0", "1-3+2"]),
            });

            useLobbyStore.getState().clearSeeks();

            expect(useLobbyStore.getState().seeks.size).toBe(0);
        });

        it("should remove all open seeks", () => {
            const openSeekTracker = new OpenSeekTracker();
            openSeekTracker.addSeeks([
                createFakeOpenSeek(),
                createFakeOpenSeek(),
            ]);
            useLobbyStore.setState({ openSeekTracker });

            useLobbyStore.getState().clearSeeks();

            expect(
                useLobbyStore.getState().openSeekTracker.interleavedOpenSeeks
                    .length,
            ).toBe(0);
        });
    });

    describe("addSeek", () => {
        it("should add a seek to the store", () => {
            const seekKey: PoolKeyStr = "0-10+0";

            useLobbyStore.getState().addSeek(seekKey);

            expect(useLobbyStore.getState().seeks.has(seekKey)).toBe(true);
        });
    });

    describe("removeSeek", () => {
        it("should remove a seek from the store", () => {
            const seekKey: PoolKeyStr = "0-10+0";
            useLobbyStore.setState({
                seeks: new Set<PoolKeyStr>([seekKey]),
            });

            useLobbyStore.getState().removeSeek(seekKey);

            expect(useLobbyStore.getState().seeks.has(seekKey)).toBe(false);
        });
    });

    describe("setRequestedOpenSeek", () => {
        it("should set requestedOpenSeek to true", () => {
            useLobbyStore.setState({ requestedOpenSeek: false });

            useLobbyStore.getState().setRequestedOpenSeek(true);

            expect(useLobbyStore.getState().requestedOpenSeek).toBe(true);
        });

        it("should set requestedOpenSeek to false", () => {
            useLobbyStore.setState({ requestedOpenSeek: true });

            useLobbyStore.getState().setRequestedOpenSeek(false);

            expect(useLobbyStore.getState().requestedOpenSeek).toBe(false);
        });
    });

    describe("addOngoingGames", () => {
        it("should add new games to ongoingGames", () => {
            const game1 = createFakeOngoingGame();
            const game2 = createFakeOngoingGame();

            useLobbyStore.getState().addOngoingGames([game1, game2]);

            expect(useLobbyStore.getState().ongoingGames).toEqual(
                new Map([
                    [game1.gameToken, game1],
                    [game2.gameToken, game2],
                ]),
            );
        });
    });

    describe("removeOngoingGame", () => {
        it("should remove a game from ongoingGames", () => {
            const gameTokenToRemove = "to remove";
            const gameToKeep = createFakeOngoingGame();

            useLobbyStore.setState({
                ongoingGames: new Map([
                    [gameToKeep.gameToken, gameToKeep],
                    [
                        gameTokenToRemove,
                        createFakeOngoingGame({ gameToken: gameTokenToRemove }),
                    ],
                ]),
            });

            useLobbyStore.getState().removeOngoingGame(gameTokenToRemove);

            expect(useLobbyStore.getState().ongoingGames).toEqual(
                new Map([[gameToKeep.gameToken, gameToKeep]]),
            );
        });
    });

    describe("addOpenSeeks", () => {
        it("should add all open seeks", () => {
            const seeks = [
                createFakeOpenSeek(),
                createFakeOpenSeek(),
                createFakeOpenSeek(),
            ];

            useLobbyStore.getState().addOpenSeeks(seeks);

            expect(
                useLobbyStore.getState().openSeekTracker.interleavedOpenSeeks,
            ).toEqual(seeks);
        });
    });

    describe("removeOpenSeek", () => {
        it("should remove the seek", () => {
            const seekToRemove = createFakeOpenSeek();
            const otherSeek1 = createFakeOpenSeek();
            const otherSeek2 = createFakeOpenSeek();

            useLobbyStore
                .getState()
                .addOpenSeeks([otherSeek1, seekToRemove, otherSeek2]);

            useLobbyStore
                .getState()
                .removeOpenSeek(seekToRemove.userId, seekToRemove.pool);

            expect(
                useLobbyStore.getState().openSeekTracker.interleavedOpenSeeks,
            ).toEqual([otherSeek1, otherSeek2]);
        });
    });
});
