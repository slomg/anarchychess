import { useState } from "react";

export default function useConst<T>(factory: () => T): T {
    const [value] = useState(factory);
    return value;
}
