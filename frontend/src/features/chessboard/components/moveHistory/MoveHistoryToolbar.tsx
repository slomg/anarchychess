import { twMerge } from "tailwind-merge";
import React, { useRef } from "react";

import useHorizontalScroll from "@/hooks/useHorizontalScroll";

const MoveHistoryToolbar = ({
    leftActions,
    rightActions,
    className,
}: {
    leftActions?: React.ReactNode;
    rightActions?: React.ReactNode;
    className?: string;
}) => {
    const actionBarRef = useRef<HTMLDivElement | null>(null);
    useHorizontalScroll(actionBarRef);

    return (
        <div
            className={twMerge(
                `border-primary flex gap-3 overflow-auto border-b p-3
                lg:border-t lg:border-b-0`,
                className,
            )}
            data-testid="moveHistoryToolbar"
            ref={actionBarRef}
        >
            {leftActions}
            <div className="ml-auto flex gap-3">{rightActions}</div>
        </div>
    );
};
export default MoveHistoryToolbar;
