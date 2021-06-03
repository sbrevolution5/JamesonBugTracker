// Toggles dark mode
function toggleDark() {
    var element = document.getElementById("layoutBody")
    element.classList.toggle("dark-mode")
    let dark = JSON.parse(localStorage.getItem("jamesonDarkMode")) 
    if (dark) {
        localStorage.setItem("jamesonDarkMode", JSON.stringify(false))
        console.log("Dark mode off")
    }
    else {
        localStorage.setItem("jamesonDarkMode", JSON.stringify(true))
        console.log("Dark mode on")

    }
}
//Checks localstorage for dark mode toggler obj, and creates one if it isn't there
function loadDark() {
    //default is light mode
    console.log("dark mode is ", JSON.parse(localStorage.getItem("jamesonDarkMode")))
    let dark = JSON.parse(localStorage.getItem("jamesonDarkMode"))
    if (dark === null) {
        localStorage.setItem("jamesonDarkMode", JSON.stringify(false))
    }
    else if (dark === true) {
        document.getElementById("layoutBody").classList.add("dark-mode")
    }
}
