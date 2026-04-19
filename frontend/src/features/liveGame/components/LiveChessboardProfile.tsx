import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import MaterialCount from "@/features/chessboard/components/MaterialCount";
import ProfilePicture from "@/features/profile/components/ProfilePicture";
import ProfileTooltip from "@/features/profile/components/ProfileTooltip";
import useLiveChessStore from "../hooks/useLiveChessStore";
import Flag from "@/features/profile/components/Flag";
import { invertColor } from "@/lib/utils/chessUtils";
import { GameColor } from "@/lib/apiClient";
import GameClock from "./GameClock";

export enum ProfileSide {
    CurrentlyPlaying,
    Opponent,
}

const LiveChessboardProfile = ({ side }: { side: ProfileSide }) => {
    const viewingFrom = useChessboardStore((state) => state.viewingFrom);
    const color =
        side === ProfileSide.CurrentlyPlaying
            ? viewingFrom
            : invertColor(viewingFrom);

    const player = useLiveChessStore((x) =>
        color === GameColor.WHITE ? x.whitePlayer : x.blackPlayer,
    );

    return (
        <div className="flex max-w-screen min-w-0 items-center justify-between">
            <div className="flex min-w-0 items-center gap-3">
                <ProfileTooltip userId={player.userId}>
                    <ProfilePicture
                        userId={player.userId}
                        size={50}
                        minSize={50}
                    />
                </ProfileTooltip>
                <div className="flex h-12 min-w-0 flex-col">
                    <div className="flex min-w-0 items-center gap-2">
                        <ProfileTooltip userId={player.userId}>
                            <p className="truncate">{player.userName}</p>
                        </ProfileTooltip>

                        <Flag countryCode={player.countryCode} size={25} />
                        {player.rating && (
                            <span
                                className="text-text/80 flex items-center
                                    rounded bg-white/10 px-2 py-1 text-xs"
                            >
                                {player.rating}
                            </span>
                        )}
                    </div>

                    <MaterialCount playerColor={color} />
                </div>
            </div>
            <GameClock color={color} />
        </div>
    );
};
export default LiveChessboardProfile;
