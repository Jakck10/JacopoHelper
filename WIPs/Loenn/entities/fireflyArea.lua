local fireflyArea = {}

fireflyArea.name = "JacopoHelper/firefly"
fireflyArea.depth = -60
fireflyArea.fillColor = {0.9, 0.9, 0.2, 0.3}
fireflyArea.borderColor = {0.9, 0.9, 0.2, 0.8}
fireflyArea.placements = {
    name = "firefly_area",
    data = {
        width = 8,
        height = 8,
        fireflyPerBlock = 5,
        brightness = 10
    }
}

return fireflyArea
