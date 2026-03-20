import clsx from "clsx";

import { MinimalProfile } from "@/lib/apiClient";
import ProfilePicture from "./ProfilePicture";
import ProfileTooltip from "./ProfileTooltip";
import { twMerge } from "tailwind-merge";

const MinimalProfileView = ({
    profile,
    index = 0,
    children,
    className,
}: {
    profile: MinimalProfile;
    index?: number;
    children?: React.ReactNode;
    className?: string;
}) => {
    return (
        <div
            className={twMerge(
                clsx(
                    `flex w-full min-w-0 flex-wrap items-center gap-3 rounded-md
                    p-3`,
                    index % 2 === 0 ? "bg-white/5" : "bg-white/15",
                ),
                className,
            )}
            data-testid="minimalProfileRow"
        >
            <ProfileTooltip userId={profile.userId}>
                <ProfilePicture userId={profile.userId} size={80} />
                <p
                    className="truncate text-lg"
                    data-testid="minimalProfileRowUsername"
                >
                    {profile.userName}
                </p>
            </ProfileTooltip>

            {children}
        </div>
    );
};
export default MinimalProfileView;
