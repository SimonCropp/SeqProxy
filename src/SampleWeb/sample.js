// begin-snippet: StructuredLogConfig
var levelSwitch = new structuredLog.DynamicLevelSwitch("info");
const log = structuredLog.configure()
    .writeTo(new structuredLog.ConsoleSink())
    .minLevel(levelSwitch)
    .writeTo(SeqSink({
        url: "http://localhost:5341",
        compact: true,
        levelSwitch: levelSwitch
    }))
    .create();
// end-snippet

// begin-snippet: StructuredLog
function LogStructured() {
    const textInput = document.getElementById("textInput").value;
    log.info('StructuredLog input: {Text}', textInput);
}
// end-snippet

// begin-snippet: LogRawJs
function LogRawJs() {
    const textInput = document.getElementById("textInput").value;
    const postSettings = {
        method: 'POST',
        credentials: 'include',
        mode: 'cors',
        body: `{'@mt':'RawJs input: {Text}','Text':'${textInput}'}`
    };

    return fetch('/api/events/raw', postSettings);
}
// end-snippet