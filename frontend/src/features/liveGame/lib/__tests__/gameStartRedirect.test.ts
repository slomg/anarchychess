import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import gameStartRedirect from "../gameStartRedirect";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";
import constants from "@/lib/constants";

vi.mock("@/features/audio/audioPlayer");

describe("gameStartRedirect", () => {
    it("should play GAME_START audio before navigation", async () => {
        const routerMock = mockRouter();
        const gameToken = "abc123";

        const promise = gameStartRedirect(gameToken, routerMock);

        expect(AudioPlayer.playAudio).toHaveBeenCalledExactlyOnceWith(
            AudioType.GAME_START,
        );
        expect(routerMock.push).not.toHaveBeenCalled();

        await promise;

        expect(routerMock.push).toHaveBeenCalledWith(
            `${constants.PATHS.GAME}/${gameToken}`,
        );
    });
});
