##vert

attribute vec2 pos;
attribute float size;

void main(void)
{
    gl_Position = vec4(pos.x, pos.y, 0.0, 1.0);
    gl_PointSize = 15.0;
}

##frag

precision highp float;
uniform vec4 pointColor;
void main(void)
{
    gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
}