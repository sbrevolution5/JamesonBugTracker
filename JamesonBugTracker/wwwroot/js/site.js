// Toggles dark mode
function toggleDark() {
    var element = document.getElementById("layoutBody")
    element.classList.toggle("dark-mode")
    var buttonElement = document.getElementById("darkIcon")
    buttonElement.classList.toggle("fa-moon")
    buttonElement.classList.toggle("fas")
    buttonElement.classList.toggle("far")
    buttonElement.classList.toggle("fa-sun")
    let dark = JSON.parse(localStorage.getItem("jamesonDarkMode"))
    if (dark) {
        localStorage.setItem("jamesonDarkMode", JSON.stringify(false))
    }
    else {
        localStorage.setItem("jamesonDarkMode", JSON.stringify(true))
    }
    //Only needs to be used with graphs
    if (page) {

        if (page == "ProjectDetails") {

            darkSwap() // changes color of graph
        }
    }
}
//Checks localstorage for dark mode toggler obj, and creates one if it isn't there
function loadDark() {
    //default is light mode
    let dark = JSON.parse(localStorage.getItem("jamesonDarkMode"))
    if (dark === null) {
        localStorage.setItem("jamesonDarkMode", JSON.stringify(true))
    }
    else if (dark === false) {
        document.getElementById("layoutBody").classList.toggle("dark-mode")
    }
    if (page == "ProjectDetails") {

        darkSwap()
    }
}
