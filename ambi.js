var top = document.getElementById("top")
        var right = document.getElementById("right")
        var bottom = document.getElementById("bottom")
        var left = document.getElementById("left")
        const width = 25;
        const urlParams = new URLSearchParams(window.location.search);
        const wait = 1000 / parseInt(urlParams.get('fps'))

        setInterval(() => {
            fetch("/ambilight").then(res => res.json().then(res => {
                var topHTML = ""
                var topWidth = 100 / res.top.length
                var rightHeight = 100 / res.right.length
                var leftHeight = 100 / res.left.length
                var bottomWidth = 100 / res.bottom.length
                var i = 0
                var op = 0.6
                res.top.forEach(e => {
                    i++
                    topHTML += `
                        <div style="background-color: #${e.hex}; width: ${topWidth}vw; height: ${width}vh; ${i * topWidth < width || i * topWidth > 100 - width ? "opacity: " + op + ";": ""}">
                        </div>
                    
                    `
                })
                document.getElementById("top").innerHTML = topHTML

                var rightHTML = ""
                
                i = 0
                res.right.forEach(e => {
                    i++
                    rightHTML += `
                        <div style="background-color: #${e.hex}; width: ${width}vw; height: ${rightHeight}vh; ${i * rightHeight < width || i * rightHeight > 100 - width ? "opacity: " + op + ";" : ""}">
                        </div>
                    
                    `
                })
                document.getElementById("right").innerHTML = rightHTML

                var bottomHTML = ""
                
                i = 0
                res.bottom.forEach(e => {
                    i++
                    bottomHTML += `
                        <div style="background-color: #${e.hex}; width: ${bottomWidth}vw; height: ${width}vh; ${i * bottomWidth < width || i * bottomWidth > 100 - width ? "opacity: " + op + ";": ""}">
                        </div>
                    
                    `
                })
                document.getElementById("bottom").innerHTML = bottomHTML

                var leftHTML = ""
                
                i = 0
                res.left.forEach(e => {
                    i++
                    leftHTML += `
                        <div style="background-color: #${e.hex}; width: ${width}vw; height: ${leftHeight}vh; ${i * leftHeight < width || i * leftHeight > 100 - width ? "opacity: " + op + ";": ""}">
                        </div>
                    
                    `
                })
                document.getElementById("left").innerHTML = leftHTML
            }))
        }, wait)