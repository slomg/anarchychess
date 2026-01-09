import clsx from "clsx";
import React from "react";

const ChessboardWithSidebar = ({
    chessboard,
    aside,
    prioritizeAside,
}: {
    chessboard: React.ReactNode;
    aside: React.ReactNode;
    prioritizeAside?: boolean;
}) => {
    return (
        <main
            className={clsx(
                `flex min-w-0 flex-1 items-center justify-center gap-5 p-5
                lg:max-h-screen lg:flex-row lg:items-start`,
                prioritizeAside ? "flex-col-reverse" : "flex-col",
            )}
        >
            <section className="flex h-max flex-col gap-3">
                {chessboard}
            </section>
            {aside}
        </main>
    );
};
export default ChessboardWithSidebar;
