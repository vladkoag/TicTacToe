/// <reference path="../../Scripts/jquery-1.6.4.js" />

$(function () {
    "use strict";

    var game = $.connection.TicTacToeHub;

    var playerSymbols = ["x", "o"];
    var gameId;
    var isInGame = false;
    var turn;
    var myPlayerNumber;
    var otherPlayerNumber;

    setElements(isInGame);

    $.connection.hub.start();

    /***********************************************************************
    Client > Hub
    ***********************************************************************/

    /*    Game type selection.    */
    $("input[name='type']").click(function () {
        var element = $("input[name='type']:checked");
        console.log("Game type selected: " + element.val());

        $("#game-type-selection .hidden").hide();

        if (!isInGame) {
            element.parent().children(".hidden").show();

            switch (element.val()) {
                case "server":
                    break;
                default:
                    break;
            }
        }
    });

    /*    Creates a new game.    */
    $("#button-create").click(function () {
        gameId = $.trim($("#game-new-id").html());
        myPlayerNumber = 0;
        otherPlayerNumber = 1;

        // Call Hub's CreateNewGame() method.
        game.createNewGame(gameId)
            .done(function (success) {
                isInGame = success;
                if (success === true) {
                    addMessage("You have created game ID: " + gameId);
                    addMessage("Waiting for other player to join.");
                } else {

                    addMessage("Unable to connect. The game ID may already exist.");
                }

                setElements({ isInGame: isInGame });
            });
    });

    /*    Joins to the existing game.    */
    $("#button-join").click(function () {
        gameId = $.trim($("#game-id").val());
        myPlayerNumber = 1;
        otherPlayerNumber = 0;

        // Call Hub's Join() method.
        game.join(gameId)
            .done(function (result) {
                isInGame = result.Success;
                if (result.Success === true) {
                    addMessage("You have joined the game ID: " + gameId);
                } else {
                    addMessage(result.FailReason);
                }

                setElements({ isInGame: isInGame });
            });
    });

    /*    Leaves the game.    */
    $("#button-leave").click(function () {
        //$("input[name='type']").prop('checked', false);

        game.leave(gameId);
    });

    /*    Game board click    */
    $("#game-board td").click(function () {
        if (isInGame && turn === myPlayerNumber) {
            var id = $(this).attr("id");
            var substr = id.split("_");

            console.log("clicked on: " + substr[0] + "," + substr[1]);

            if ($(this).html() !== playerSymbols[myPlayerNumber]) {
                $(this).html(playerSymbols[myPlayerNumber]);

                // Call Hub's SelectField() method.
                game.selectField(gameId, turn, { x: parseInt(substr[0]), y: parseInt(substr[1]) });
            }
        }
    });

    /***********************************************************************
    Hub > Client
    ***********************************************************************/

    /*    In response to other player move, updates the game board.    */
    game.setOtherPlayerField = function (field) {
        $("#game-board #" + field.x + "_" + field.y).html(playerSymbols[otherPlayerNumber]);
    };

    /*    Sets the game state to end game and informs the player.    */
    game.endGame = function () {
        isInGame = false;
        addMessage("The game has ended.");
        setElements({ isInGame: isInGame });
    };

    /*    Informs Player 1 that Player 2 has joined the game.    */
    game.player2Joined = function () {
        addMessage("Player 2 joined.");
    };

    /*    Sets turn. 0=Player 1, 1=Player 2    */
    game.setTurn = function (playerNumber) {
        turn = playerNumber;
    };

    /*    Adds a new message.    */
    game.addMessage = function (message) {
        addMessage(message);
    };

    /***********************************************************************
    ***********************************************************************/

    /*    Adds a new message.    */
    function addMessage(message, player) {
        var m = message;

        if (player) {
            m = player + ": " + message;
        }

        var e = $('<li/>').html(m).appendTo($("#messages"));
        e[0].scrollIntoView();
    }

    /*    Shows/Hides elements on the page.    */
    function setElements(options) {
        if (options.isInGame) {
            $("#button-leave").show();
            $("#button-create").hide();
        } else {
            $("#button-leave").hide();
            $("#button-create").show();
        }

        $("input[name='type']").prop('checked', false);
        $("#game-type-selection .hidden").hide();
        //$("#messages").empty();
    }
});