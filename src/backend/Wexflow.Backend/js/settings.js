const Settings = (function () {
    let hostname = (window.location.hostname === "" ? "localhost" : window.location.hostname);
    let port = 8000;

    return {
        Hostname: hostname,
        Port: port,
        Uri: "http://" + hostname + ":" + port + "/wexflow/"
    };
})();