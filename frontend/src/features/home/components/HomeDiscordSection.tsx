import Image from "next/image";
import Link from "next/link";

import sniperBishopPfp from "@public/assets/home-discord/sniper-bishop-pfp.webp";
import slomgPfp from "@public/assets/home-discord/slomg-pfp.webp";
import discordIcon from "@public/assets/oauth/discord.svg";
import Button from "@/components/ui/Button";
import xdd from "@public/assets/xdd.png";
import constants from "@/lib/constants";

const HomeDiscordSection = () => {
    return (
        <section
            className="grid grid-rows-[auto_auto] justify-center gap-10
                bg-[#0b131a] p-5 lg:grid-cols-[auto_auto] lg:grid-rows-1
                lg:gap-x-10 lg:p-15"
        >
            <div
                className="flex w-full flex-col items-center gap-3 px-10
                    text-center lg:order-1 lg:max-w-lg lg:items-start lg:px-0
                    lg:text-start"
            >
                <h2 className="text-5xl">
                    Want Something{" "}
                    <span className="text-secondary">Added?</span>
                </h2>

                <p className="text-text/80 text-xl text-balance">
                    Join the Discord server to suggest new pieces, rules, report
                    bugs or just chat about anything you like!
                </p>

                <Link
                    href={constants.PATHS.DISCORD}
                    className="mt-5 w-full max-w-sm"
                >
                    <Button
                        className="flex w-full items-center justify-center gap-3
                            px-10 text-3xl text-nowrap"
                    >
                        <Image
                            src={discordIcon}
                            className="brightness-0 invert-90"
                            alt="discord logo"
                            width={40}
                            height={40}
                        />
                        JOIN DISCORD
                    </Button>
                </Link>
            </div>

            <div
                className="mx-auto my-auto flex flex-col items-center gap-4 px-4
                    lg:max-w-lg lg:items-end"
            >
                <div
                    className="flex flex-col gap-3 rounded-xl border
                        border-white/10 bg-[#1f2433] p-5 shadow-md"
                >
                    <div className="flex items-center gap-3">
                        <Image
                            src={sniperBishopPfp}
                            alt="sniper bishop pfp"
                            className="flex-shrink-0 rounded-full"
                            width={36}
                            height={36}
                        />
                        <span className="min-w-0 truncate font-medium">
                            John Bishop
                        </span>
                        <span className="text-text/50 text-xs">2:30 PM</span>
                    </div>

                    <p>
                        instead of instantly losing when you run out of time,
                        your pieces just start leaving
                    </p>
                </div>

                <div
                    className="flex w-full flex-col gap-3 rounded-xl border
                        border-white/5 bg-[#1f2433] p-5 shadow-md lg:w-md"
                >
                    <div className="flex items-center gap-3">
                        <Image
                            src={slomgPfp}
                            alt="slomg pfp"
                            className="flex-shrink-0 rounded-full"
                            width={36}
                            height={36}
                        />
                        <span className="min-w-0 truncate font-medium">
                            slomg
                        </span>
                        <span className="text-text/50 text-xs">2:33 PM</span>
                    </div>

                    <p className="flex items-center gap-2">
                        omg i love this{" "}
                        <Image
                            src={xdd}
                            alt="xdd"
                            width={20}
                            height={20}
                            className="inline-block"
                        />
                    </p>
                </div>
            </div>
        </section>
    );
};
export default HomeDiscordSection;
