import { Metadata } from "next";
import Link from "next/link";

import ChangeLogCard, {
    ChangeLogType,
} from "@/features/changelog/components/ChangeLogCard";

import ChangeLogMonthDivider from "@/features/changelog/components/ChangeLogMonthDivider";
import constants from "@/lib/constants";

export const metadata: Metadata = {
    title: "Change Log - Anarchy Chess",
    description: "Stay up to date with the latest Anarchy Chess updates.",
    keywords: [
        "anarchy chess updates",
        "chess variant updates",
        "new chess rules",
        "new chess features",
        "anarchychess game",
    ],
};

export default function ChangeLogPage() {
    return (
        <main className="mx-auto flex w-full max-w-7xl flex-col gap-5 p-10">
            <section className="flex flex-col gap-3">
                <h1 className="text-6xl">Change Log</h1>
                <Link
                    className="text-text/80 w-fit text-lg"
                    href={constants.PATHS.DISCORD}
                >
                    <h2>Join the Discord for near daily updates!</h2>
                </Link>
            </section>

            <ChangeLogMonthDivider date="May 2026" />

            <ChangeLogCard type={ChangeLogType.RULE} date="May 10">
                alksdjlkjh asdlkjasdkjla sdlkj asd lkj asd lkj asd lkj asd
                lkjasdlkjaskjdh ALJDh aKSLJHdf alkjh AKLJfh KLADSJFh ALKSJHD
                lkaSJHD KALJSDH akldjh akjlHD kJASHD kajH Dka LKJSDlaKSDJalksjd
                LAKSJd LKA:SJ d;lKASJd ;laKJSD ;aklJSHd o
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FIX} date="May 2">
                alksdjlkjh asdlkjasdkjla sdlkj asd lkj asd lkj asd lkj asd
                lkjasdlkjaskjdh ALJDh aKSLJHdf alkjh AKLJfh KLADSJFh ALKSJHD
                lkaSJHD KALJSDH akldjh akjlHD kJASHD kajH Dka LKJSDlaKSDJalksjd
                LAKSJd LKA:SJ d;lKASJd ;laKJSD ;aklJSHd o
            </ChangeLogCard>

            <ChangeLogMonthDivider date="April 2026" />

            <ChangeLogCard type={ChangeLogType.TWEAK} date="April 29">
                alksdjlkjh asdlkjasdkjla sdlkj asd lkj asd lkj asd lkj asd
                lkjasdlkjaskjdh ALJDh aKSLJHdf alkjh AKLJfh KLADSJFh ALKSJHD
                lkaSJHD KALJSDH akldjh akjlHD kJASHD kajH Dka LKJSDlaKSDJalksjd
                LAKSJd LKA:SJ d;lKASJd ;laKJSD ;aklJSHd o
            </ChangeLogCard>

            <ChangeLogCard type={ChangeLogType.FEATURE} date="April 26">
                alksdjlkjh asdlkjasdkjla sdlkj asd lkj asd lkj asd lkj asd
                lkjasdlkjaskjdh ALJDh aKSLJHdf alkjh AKLJfh KLADSJFh ALKSJHD
                lkaSJHD KALJSDH akldjh akjlHD kJASHD kajH Dka LKJSDlaKSDJalksjd
                LAKSJd LKA:SJ d;lKASJd ;laKJSD ;aklJSHd o
            </ChangeLogCard>
        </main>
    );
}
