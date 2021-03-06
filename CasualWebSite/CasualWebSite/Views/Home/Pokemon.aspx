﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Model.Web.PokemonModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    TestPage
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% using (Html.BeginForm())
       { %>
    <div>
        <%= Html.TextAreaFor(x => x.Query) %>
    </div>
    <button>Send query</button>
    <% } %>

    <% if (Model.ErrorExecutingSql)
       { %>
       <p>Error executing query</p>
    <% } %>

    <br />
    <table>
        <thead>
            <tr>
                <th>
                    <a href="http://bulbapedia.bulbagarden.net/wiki/National_Pok%C3%A9dex" title="National Pokédex">
                        #</a>
                </th>
                <th>
                    &nbsp;
                </th>
                <th>
                    Pokémon
                </th>
                <th class="headerSort" style="background: rgb(255, 0, 0); -moz-border-radius-topleft: 10px;
                    -moz-border-radius-topright: 10px;">
                    <small>HP</small>
                </th>
                <th class="headerSort" style="background: rgb(240, 128, 48); -moz-border-radius-topleft: 10px;
                    -moz-border-radius-topright: 10px;">
                    <small>Attack</small>
                </th>
                <th class="headerSort" style="background: rgb(248, 208, 48); -moz-border-radius-topleft: 10px;
                    -moz-border-radius-topright: 10px;">
                    <small>Defense</small>
                </th>
                <th class="headerSort" style="background: rgb(104, 144, 240); -moz-border-radius-topleft: 10px;
                    -moz-border-radius-topright: 10px;">
                    <small>Sp. Attack</small>
                </th>
                <th class="headerSort" style="background: rgb(120, 200, 80); -moz-border-radius-topleft: 10px;
                    -moz-border-radius-topright: 10px;">
                    <small>Sp. Defense</small>
                </th>
                <th class="headerSort" style="background: rgb(248, 88, 136); -moz-border-radius-topleft: 10px;
                    -moz-border-radius-topright: 10px;">
                    <small>Speed</small>
                </th>
                <th class="headerSort">
                    <small>Total</small>
                </th>
                <th class="headerSort">
                    <small>Type</small>
                </th>
            </tr>
        </thead>
        <tbody>
            <% foreach (var stat in Model.StatList)
               { %>
            <tr>
                <td>
                    <%= stat.id %>
                </td>
                <td>
                    <img src="/Content/image/pokemonImage/<%= stat.id %>MS.png" alt="" />
                </td>
                <td>
                    <%= stat.name %>
                </td>
                <td>
                    <%= stat.hp %>
                </td>
                <td>
                    <%= stat.attack %>
                </td>
                <td>
                    <%= stat.defense %>
                </td>
                <td>
                    <%= stat.spattack %>
                </td>
                <td>
                    <%= stat.spdefense %>
                </td>
                <td>
                    <%= stat.speed %>
                </td>
                <td>
                    <%= stat.total %>
                </td>
                <td>
                    <img src="/Content/image/pokemonType/<%=stat.type1 %>IC.gif" alt="" />
                    <img src="/Content/image/pokemonType/<%=stat.type2 %>IC.gif" alt="" />
                </td>
            </tr>
            <% } %>
        </tbody>
    </table>
</asp:Content>
