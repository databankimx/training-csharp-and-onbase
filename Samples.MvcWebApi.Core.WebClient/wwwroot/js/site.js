/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */

// *Migration Note: the classic web clients (Samples.AsmxWebService.WebClient,
//   Samples.WcfService.WebClient, Samples.MvcWebApi.WebClient) all use jQuery's $.ajax(),
//   the dominant browser HTTP pattern when those services were originally written. This
//   client uses the browser's native fetch() with async/await instead, no library needed
//   at all for a modern browser, another deliberate, direct contrast alongside
//   Samples.MvcWebApi.Core.Client's HttpClient-vs-HttpWebRequest comparison on the .NET
//   side. See LectureNotes.md.

const apiBaseUrl = "https://localhost:44314/api/";

const requestPane = document.getElementById("requestJson");
const responsePane = document.getElementById("responseJson");

document.getElementById("btnPing").addEventListener("click", () => callApi("GET", "ping"));

document.getElementById("btnTest").addEventListener("click", () => {
    const data = document.getElementById("testData").value;
    if (!validate(data, 1, "test data")) return;
    callApi("POST", "test", { requestId: createGuid(), data });
});

document.getElementById("btnLookupLocation").addEventListener("click", () => {
    const zipCode = document.getElementById("zipCode").value;
    if (!validate(zipCode, 5, "zip code")) return;
    callApi("GET", `locationlookup/${encodeURIComponent(zipCode)}`);
});

function validate(value, minLength, name) {
    requestPane.textContent = "";
    responsePane.textContent = "";
    responsePane.classList.remove("error");
    if (!value || value.length < minLength) {
        alert(`You must include at least ${minLength} character${minLength === 1 ? "" : "s"} in the ${name} field!`);
        return false;
    }
    return true;
}

async function callApi(method, path, body) {
    const url = apiBaseUrl + path;
    requestPane.textContent = body ? JSON.stringify(body, null, 2) : "(no body)";
    console.log(`Sending ${method} ${url}`, body ?? "");

    try {
        const response = await fetch(url, {
            method,
            headers: body ? { "Content-Type": "application/json" } : undefined,
            body: body ? JSON.stringify(body) : undefined
        });

        const text = await response.text();
        let displayText = text;
        try {
            displayText = JSON.stringify(JSON.parse(text), null, 2);
        } catch {
            // Ping returns a plain string, not JSON, display as-is
        }

        if (!response.ok) {
            responsePane.classList.add("error");
            console.error(`Request failed: ${response.status} ${response.statusText}`, text);
        } else {
            console.log(`Received ${response.status}`, text);
        }

        responsePane.textContent = displayText;
    } catch (ex) {
        responsePane.classList.add("error");
        responsePane.textContent = String(ex);
        console.error("Error calling API!", ex);
    }
}

function createGuid() {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, c => {
        const r = Math.random() * 16 | 0;
        const v = c === "x" ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
