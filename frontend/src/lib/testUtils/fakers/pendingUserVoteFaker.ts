import { PendingUserVote } from "@/lib/apiClient";
import { createFakeVoteOption } from "./voteOptionFaker";

export function createFakePendingUserVote(
    overrides?: Partial<PendingUserVote>,
): PendingUserVote {
    return {
        optionA: createFakeVoteOption(),
        optionB: createFakeVoteOption(),
        ...overrides,
    };
}
