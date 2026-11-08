local uCoreBerry = {}

uCoreBerry.name = "JacopoHelper/UntrackedCoreBerry"
uCoreBerry.depth = -100
uCoreBerry.placements = {
    name = "UCB",
    data = {
        order = -1,
        checkpointID = -1,
        isIce = false,
    }
}

function uCoreBerry.texture(room, entity)
    return entity.isIce
        and "collectables/FrostHelper/CoreBerry/Cold/ColdBerry_Cold00"
        or "collectables/FrostHelper/CoreBerry/Hot/CoreBerry_Hot00"
end

return uCoreBerry