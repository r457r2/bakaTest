#version 330

layout(location = 0) in vec4 pos;
layout(location = 1) in vec4 col;

uniform mat4 model;
uniform mat4 projection;
uniform mat4 view;

out vec4 outCol;

void main()
{
    gl_Position = projection * view * model * pos;
    outCol = col;
}