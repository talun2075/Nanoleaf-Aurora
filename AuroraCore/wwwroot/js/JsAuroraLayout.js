const CENTROID_HEIGHT = Math.sqrt(3) / 6 * 150;
const PI = 3.141592653589793238462643383;

const strokeColorw =  '#ffffff';
const colorW = '#333333';
function compute(data) {
    const { positionData, sideLength } = data;
    let minX = 0;
    let maxX = 0;
    let minY = 0;
    let maxY = 0;

    positionData.forEach(panel => {
        if (panel.x > maxX) {
            maxX = panel.x
        }
        if (panel.x < minX) {
            minX = panel.x
        }
        if (panel.y > maxY) {
            maxY = panel.y
        }
        if (panel.y < minY) {
            minY = panel.y
        }
    });

    // the min/max are now based on the center of the triangles, so we want to add a sideLength so we're
    // working with the triangle bounding boxes
    maxX += sideLength;
    minX -= sideLength;
    maxY += sideLength;
    minY -= sideLength;

    const width = (maxX - minX);
    const height = (maxY - minY);

    return {
        midX: minX + width / 2,
        midY: minY + height / 2,
        minX,
        minY,
        maxX,
        maxY,
        width,
        height
    }
};

function colorAsInt(hexString) {
    if (!hexString) return 0; // cover nulls and undefined
    return parseInt(hexString.slice(1), 0x10)
};
function equilateral (sideLength, orientation, cen = [0, 0]) {

    var halfsidelenght = sideLength / 2;
    var h = (sideLength * Math.sqrt(3) / 2);
    var xdreieck = h / 3 * 2;
    var ydreieck = h / 3;
    let leftVertex = [];
    let rightVertex = [];
    let topVertex = [];
    switch (orientation) {
        case 0:
        case 360:
        case 120:
        case 240:
            leftVertex[0] = cen[0];
            leftVertex[1] = cen[1] + xdreieck;
            rightVertex[0] = cen[0] - halfsidelenght;
            rightVertex[1] = cen[1] - ydreieck;
            topVertex[0] = cen[0] + halfsidelenght;
            topVertex[1] = cen[1] - ydreieck;
            break;
        case 180:
        case 60:
        case 300:
            leftVertex[0] = cen[0] - halfsidelenght;
            leftVertex[1] = cen[1] + ydreieck;
            rightVertex[0] = cen[0] + halfsidelenght;
            rightVertex[1] = cen[1] + ydreieck;
            topVertex[0] = cen[0];
            topVertex[1] = cen[1] - xdreieck;
            break;
    }

    return {
        topVertex,
        rightVertex,
        leftVertex
    }
};
function draw (positionData, sideLength) {
    // Calculate the coords for an equilateral triangle
    let color = colorW;
    return positionData.map(({ x, y, orientation, panelId, shapeType }) => {
        if (shapeType == 0) {
            let e = equilateral(sideLength, orientation);
            console.log(e);
            let path = `M${e.topVertex[0]} ${e.topVertex[1]} L${e.leftVertex[0]} ${e.leftVertex[1]} L${e.rightVertex[0]} ${e.rightVertex[1]} L${e.topVertex[0]} ${e.topVertex[1]} Z`;
            let col = colorW;
            return {
                x,
                y,
                rotation: orientation,
                colorW,
                strokeColorw,
                path,
                panelId
            };
        }
        
    });
};

function update(data) {
    //Sort panels so that strokeColor further from white are later in the array.  This prevents overlapping a non-white strokeColor with white.
    const panels = draw(data.layout.positionData, data.layout.sideLength).sort((a, b) => colorAsInt(b.strokeColor) - colorAsInt(a.strokeColor));
    return panels.map((value, key) => {
        //return ('<g key="'+key+'" transform="translate(' + value.x + ',' + value.y + ') rotate(' + value.rotation + 60 + ')"><path key="' + key + '" d="' + value.path + '" fill="#333333" stroke="#ffffff" /><text key="' + key + '" fill="#FFFFFF" textAnchor="middle" transform="scale(-1, 1) rotate('+(value.rotation-120)+')">' + value.panelId +'</text></g>');
        return ('<g key="' + key + '" transform="translate(' + value.x + ',' + value.y + ')"><path key="' + key + '" d="' + value.path + '" fill="#333333" stroke="#ffffff" /><text key="' + key + '" fill="#FFFFFF" textAnchor="middle" transform="scale(-1, 1)">' + value.panelId + '</text></g>');
    });
};




function render(data) {
    const { midX, midY, minX, minY, width, height } = compute(data.layout);

    //Translate out, scale and rotate, translate back.  Makes it 'feel' like the scale and rotation are happening around the center and not around 0,0
    const transform = `translate(${midX},${midY}) scale(-1,1) rotate(180) translate(${-midX},${-midY})`;

    // Use calculated to give a tight view of the panels
    const viewBox = `${minX} ${minY} ${width} ${height}`;

    return (
        '<svg viewBox="' + viewBox + '" style ="backgroundColor: blue" preserveAspectRatio="xMidYMid meet" ><g transform="' + transform + '"> '+update(data)+'</g></svg>'
    )
}

function test() {
    var testdom = document.getElementById("test");
    var svg = window.render(window.aurora.d[0].nlj.panelLayout);
    testdom.innerHTML = svg;
}