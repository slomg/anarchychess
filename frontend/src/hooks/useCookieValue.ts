import { useEffect, useEffectEvent, useState } from "react";
import Cookies from "js-cookie";

export default function useCookieValue<T>(
    cookieName: string,
    initialValue: T,
): T {
    const [value, setValue] = useState(initialValue);

    const setValueFromCookie = useEffectEvent(() => {
        const cookieValue = Cookies.get(cookieName);
        if (!cookieValue) return;
        setValue(JSON.parse(cookieValue));
    });
    useEffect(() => setValueFromCookie(), []);

    return value;
}
