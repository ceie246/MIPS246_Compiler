﻿<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="submitHomework.aspx.cs" Inherits="submitHomework" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageBody" Runat="Server">
   <form id="form1" runat="server">
        <div class="container">
    <div id="subtitle">
            <h1 class="mips246font">Mips246 作业列表</h1>
        </div>
        <table class="table table-hover">
            <thead>
                <tr>
                    <th>学号</th>
                    <th>姓名</th>
                    <th>作业1</th>
                    <th>作业2</th>
                    <th>作业3</th>
                    <th>作业4</th>
                    <th>作业5</th>
                    <th>作业6</th>
                    <th>作业7</th>
                    <th>作业8</th>
                    <th>作业9</th>
                    <th>作业10</th>
                </tr>
            </thead>
            <tr>
                <%=homeworkStatus %>
            </tr>
        </table>
       </div>
    </form>
</asp:Content>

