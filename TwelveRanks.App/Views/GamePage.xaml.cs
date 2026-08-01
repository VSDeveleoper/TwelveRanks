using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwelveRanks.App.Controls;
using TwelveRanks.App.Services;
using TwelveRanks.Computer;
using TwelveRanks.Core;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;

namespace TwelveRanks.App.Views;

internal sealed partial class GamePage : Page
{
    private const int CapturedPieceColumnCount = 3;
    private const double CoordinateGutterSize = 22;
    private const double DefaultCapturedPieceSize = 60;

    private static readonly SolidColorBrush LightSquareBrush =
        new(ColorHelper.FromArgb(255, 238, 217, 181));

    private static readonly SolidColorBrush DarkSquareBrush =
        new(ColorHelper.FromArgb(255, 108, 142, 87));

    private static readonly SolidColorBrush GridLineBrush =
        new(ColorHelper.FromArgb(70, 40, 34, 27));

    private static readonly SolidColorBrush SelectedSquareBrush =
        new(ColorHelper.FromArgb(255, 255, 193, 7));

    private static readonly SolidColorBrush LegalMoveBrush =
        new(ColorHelper.FromArgb(210, 29, 121, 84));

    private static readonly SolidColorBrush CaptureMoveBrush =
        new(ColorHelper.FromArgb(230, 198, 40, 40));

    private static readonly SolidColorBrush CheckedKingBrush =
        new(ColorHelper.FromArgb(255, 220, 20, 60));

    private static readonly SolidColorBrush MinisterExchangeBrush =
        new(ColorHelper.FromArgb(255, 25, 118, 210));

    private static readonly SolidColorBrush TransparentBrush =
        new(Colors.Transparent);

    private static readonly SolidColorBrush CoordinateTextBrush =
        new(ColorHelper.FromArgb(235, 255, 255, 255));

    private readonly Grid[][] _squareContents = CreateSquareContents();
    private readonly Border[][] _squareHighlights = CreateSquareHighlights();
    private readonly Button[][] _squareInputs = CreateSquareInputs();
    private readonly BoardPiece?[][] _renderedPieces =
        CreateRenderedPieces();
    private readonly ConventionalComputerOpponent _computerOpponent =
        new();

    private ChessGame _game = ChessGame.CreateInitial();
    private IReadOnlyList<ChessMove> _selectedMoves =
        [];
    private BoardSquare? _selectedSquare;
    private GameMode? _gameMode;
    private GameNavigationRequest _navigationRequest =
        GameNavigationRequest.ChooseMode;
    private CancellationTokenSource? _computerTurnCancellation;
    private Task? _computerTurnTask;
    private long _gameSessionId;
    private bool _isNavigationInitializationPending;
    private bool _isNavigationInitializing;
    private bool _isComputerThinking;
    private bool _isGameActionPending;
    private bool _hasSaveFailure;
    private bool _isActivePage;
    private string? _computerTurnMessage;

    public GamePage()
    {
        InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Required;
        CreateEmptyBoard();
        CreateBoardCoordinates();
        ApplyBoardCoordinatePreference();
        RenderGame(true);
        Loaded += GamePage_Loaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnNavigatedTo(e);

        _isActivePage = true;
        CancelComputerTurn();
        _computerTurnMessage = null;
        _navigationRequest = e.Parameter switch
        {
            GameNavigationRequest request => request,
            GameMode mode => GameNavigationRequest.StartNew(mode),
            _ => GameNavigationRequest.ChooseMode
        };
        _gameMode = null;
        _game = ChessGame.CreateInitial();
        _selectedSquare = null;
        _selectedMoves = [];
        _isNavigationInitializationPending = true;

        ApplyBoardCoordinatePreference();
        UpdateModeText();
        RenderGame(true);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        _isNavigationInitializationPending = false;
        _isActivePage = false;
        UpdateHistoryCommandState();
        CancelComputerTurn();
        base.OnNavigatedFrom(e);
    }

    private async void GamePage_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(e);

        if (!_isNavigationInitializationPending ||
            _isNavigationInitializing)
        {
            return;
        }

        _isNavigationInitializationPending = false;
        _isNavigationInitializing = true;
        try
        {
            await InitializeNavigationAsync().ConfigureAwait(true);
        }
        finally
        {
            _isNavigationInitializing = false;
        }
    }

    private async Task InitializeNavigationAsync()
    {
        SavedGame? savedGame =
            await GameSaveService.TryLoadAsync().ConfigureAwait(true);

        if (await TryResolveSavedGameAsync(
                savedGame).ConfigureAwait(true))
        {
            return;
        }

        if (savedGame is null &&
            IsSupportedMode(_navigationRequest.RequestedMode))
        {
            await StartNewGameAsync(
                _navigationRequest.RequestedMode!.Value).ConfigureAwait(true);
            return;
        }

        GameMode? selectedMode =
            await GameDialogService.ShowGameModeAsync(
                XamlRoot).ConfigureAwait(true);

        if (selectedMode is GameMode mode)
        {
            await StartNewGameAsync(mode).ConfigureAwait(true);
        }
        else
        {
            NavigateHome();
        }
    }

    private async Task<bool> TryResolveSavedGameAsync(
        SavedGame? savedGame)
    {
        if (savedGame is null)
        {
            return false;
        }

        if (_navigationRequest.ResumeSavedGame)
        {
            ApplyGame(savedGame.Game, savedGame.Mode);
            return true;
        }

        bool? shouldResume =
            await GameDialogService.ShowResumeGameAsync(
                XamlRoot).ConfigureAwait(true);

        if (shouldResume == true)
        {
            ApplyGame(savedGame.Game, savedGame.Mode);
            return true;
        }

        if (!shouldResume.HasValue)
        {
            NavigateHome();
            return true;
        }

        return false;
    }

    private async Task StartNewGameAsync(GameMode mode)
    {
        if (!await GameSaveService.ClearAsync().ConfigureAwait(true))
        {
            await GameDialogService.ShowSaveUnavailableAsync(
                XamlRoot).ConfigureAwait(true);
            NavigateHome();
            return;
        }

        ApplyGame(ChessGame.CreateInitial(), mode);
    }

    private void ApplyGame(ChessGame game, GameMode mode)
    {
        _game = game;
        _gameMode = mode;
        _selectedSquare = null;
        _selectedMoves = [];
        _hasSaveFailure = false;
        _computerTurnMessage = null;
        RenderGame(true);
        UpdateModeText();
        StartComputerTurnIfNeeded();
    }

    private static bool IsSupportedMode(GameMode? mode)
    {
        return mode is GameMode.LocalTwoPlayer or GameMode.Computer;
    }

    private static void NavigateHome()
    {
        ((ChessApplication)Application.Current)
            .MainWindow
            .NavigateToHome();
    }

    private void CreateEmptyBoard()
    {
        for (int index = 0; index < BoardSpecification.Ranks; index++)
        {
            BoardGrid.RowDefinitions.Add(new RowDefinition());
        }

        for (int index = 0; index < BoardSpecification.Files; index++)
        {
            BoardGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        for (int rank = 0; rank < BoardSpecification.Ranks; rank++)
        {
            for (int file = 0; file < BoardSpecification.Files; file++)
            {
                CreateSquare(new BoardSquare(rank, file));
            }
        }
    }

    private void CreateBoardCoordinates()
    {
        for (int file = 0; file < BoardSpecification.Files; file++)
        {
            TopFileCoordinates.ColumnDefinitions.Add(
                new ColumnDefinition());
            BottomFileCoordinates.ColumnDefinitions.Add(
                new ColumnDefinition());

            string label = ((char)('A' + file)).ToString();
            AddFileCoordinate(
                TopFileCoordinates,
                file,
                label);
            AddFileCoordinate(
                BottomFileCoordinates,
                file,
                label);
        }

        for (int rank = 0; rank < BoardSpecification.Ranks; rank++)
        {
            LeftRankCoordinates.RowDefinitions.Add(
                new RowDefinition());
            RightRankCoordinates.RowDefinitions.Add(
                new RowDefinition());

            string notation = new BoardSquare(rank, 0).Notation;
            string label = notation[1..];
            AddRankCoordinate(
                LeftRankCoordinates,
                rank,
                label);
            AddRankCoordinate(
                RightRankCoordinates,
                rank,
                label);
        }
    }

    private static void AddFileCoordinate(
        Grid target,
        int file,
        string label)
    {
        TextBlock coordinate = CreateCoordinateLabel(label);
        Grid.SetColumn(coordinate, file);
        target.Children.Add(coordinate);
    }

    private static void AddRankCoordinate(
        Grid target,
        int rank,
        string label)
    {
        TextBlock coordinate = CreateCoordinateLabel(label);
        Grid.SetRow(coordinate, rank);
        target.Children.Add(coordinate);
    }

    private static TextBlock CreateCoordinateLabel(string label)
    {
        return new TextBlock
        {
            Text = label,
            FontSize = 13,
            Foreground = CoordinateTextBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private void ApplyBoardCoordinatePreference()
    {
        bool showCoordinates =
            AppSettingsService.ShowBoardCoordinates;
        GridLength gutter = new(
            showCoordinates ? CoordinateGutterSize : 0);
        Visibility visibility = showCoordinates
            ? Visibility.Visible
            : Visibility.Collapsed;

        TopCoordinateRow.Height = gutter;
        BottomCoordinateRow.Height = gutter;
        LeftCoordinateColumn.Width = gutter;
        RightCoordinateColumn.Width = gutter;
        TopFileCoordinates.Visibility = visibility;
        BottomFileCoordinates.Visibility = visibility;
        LeftRankCoordinates.Visibility = visibility;
        RightRankCoordinates.Visibility = visibility;
    }

    private void CreateSquare(BoardSquare boardSquare)
    {
        Grid pieceContent = new();
        Border highlight = new()
        {
            BorderThickness = new Thickness(0),
            IsHitTestVisible = false
        };
        Button input = new()
        {
            Background = TransparentBrush,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            Padding = new Thickness(0),
            Tag = boardSquare,
            TabIndex = boardSquare.Index
        };

        input.Click += OnSquareClicked;
        AutomationProperties.SetAutomationId(
            input,
            $"BoardSquare_r{boardSquare.Rank}_f{boardSquare.File}");

        Grid layers = new();
        layers.Children.Add(pieceContent);
        layers.Children.Add(highlight);
        layers.Children.Add(input);

        Border square = new()
        {
            Background = GetBaseSquareBrush(boardSquare),
            BorderBrush = GridLineBrush,
            BorderThickness = new Thickness(0.5),
            Child = layers
        };

        Grid.SetRow(square, boardSquare.Rank);
        Grid.SetColumn(square, boardSquare.File);
        BoardGrid.Children.Add(square);

        _squareContents[boardSquare.Rank][boardSquare.File] = pieceContent;
        _squareHighlights[boardSquare.Rank][boardSquare.File] = highlight;
        _squareInputs[boardSquare.Rank][boardSquare.File] = input;
    }

    private async void OnSquareClicked(object sender, RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(e);

        if (sender is Button { Tag: BoardSquare square })
        {
            await HandleSquareClickAsync(square).ConfigureAwait(true);
        }
    }

    private async Task HandleSquareClickAsync(BoardSquare square)
    {
        if (!CanHumanUseBoard())
        {
            return;
        }

        if (_selectedSquare == square)
        {
            ClearSelection();
            return;
        }

        if (await TryApplySelectedMoveAsync(square).ConfigureAwait(true))
        {
            return;
        }

        SelectOrClearSquare(square);
    }

    private async Task<bool> TryApplySelectedMoveAsync(BoardSquare square)
    {
        ChessMove[] matchingMoves = [.. _selectedMoves.Where(move => move.To == square)];

        if (matchingMoves.Length == 0)
        {
            return false;
        }

        ChessMove? selectedMove =
            await ResolveMoveAsync(matchingMoves).ConfigureAwait(true);

        if (selectedMove.HasValue &&
            await TryApplyMoveAsync(selectedMove.Value).ConfigureAwait(true))
        {
            StartComputerTurnIfNeeded();
        }

        return true;
    }

    private async Task<bool> TryApplyMoveAsync(ChessMove move)
    {
        if (!_game.TryMakeMove(move, out MoveResult? result) ||
            result is null)
        {
            return false;
        }

        _selectedSquare = null;
        _selectedMoves = [];
        RenderGame(false);
        MoveSoundService.Play(result);
        await PersistGameAsync().ConfigureAwait(true);
        return true;
    }

    internal async Task UndoAsync()
    {
        if (!_game.CanUndo)
        {
            return;
        }

        CancelComputerTurn();
        _computerTurnMessage = null;

        if (!_game.UndoLastMove())
        {
            return;
        }

        if (_gameMode == GameMode.Computer &&
            _game.SideToMove == PieceSide.Black &&
            _game.CanUndo)
        {
            _ = _game.UndoLastMove();
        }

        await CompleteHistoryNavigationAsync().ConfigureAwait(true);
    }

    internal async Task RedoAsync()
    {
        if (!_game.CanRedo)
        {
            return;
        }

        CancelComputerTurn();
        _computerTurnMessage = null;

        if (!_game.RedoLastMove())
        {
            return;
        }

        if (_gameMode == GameMode.Computer &&
            _game.SideToMove == PieceSide.Black &&
            _game.CanRedo)
        {
            _ = _game.RedoLastMove();
        }

        await CompleteHistoryNavigationAsync().ConfigureAwait(true);
    }

    internal async Task EndGameAsync()
    {
        if (_gameMode is null || _isGameActionPending)
        {
            return;
        }

        _isGameActionPending = true;
        UpdateHistoryCommandState();
        try
        {
            CancelComputerTurn();

            if (!await GameDialogService.ShowEndGameConfirmationAsync(
                    XamlRoot).ConfigureAwait(true))
            {
                StartComputerTurnIfNeeded();
                return;
            }

            if (!await GameSaveService.ClearAsync().ConfigureAwait(true))
            {
                await GameDialogService.ShowEndGameUnavailableAsync(
                    XamlRoot).ConfigureAwait(true);
                StartComputerTurnIfNeeded();
                return;
            }

            NavigateHome();
        }
        finally
        {
            _isGameActionPending = false;
            UpdateHistoryCommandState();
        }
    }

    private async void WhiteResignButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(e);
        await ResignAsync(PieceSide.White).ConfigureAwait(true);
    }

    private async void BlackResignButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(e);
        await ResignAsync(PieceSide.Black).ConfigureAwait(true);
    }

    private async Task ResignAsync(PieceSide side)
    {
        bool isAllowed = _gameMode switch
        {
            GameMode.LocalTwoPlayer => true,
            GameMode.Computer => side == PieceSide.White,
            _ => false
        };

        if (!isAllowed ||
            _game.IsGameOver ||
            _isGameActionPending)
        {
            return;
        }

        _isGameActionPending = true;
        UpdateHistoryCommandState();
        try
        {
            CancelComputerTurn();

            if (AppSettingsService.ConfirmBeforeResigning &&
                !await GameDialogService.ShowResignConfirmationAsync(
                    XamlRoot,
                    side).ConfigureAwait(true))
            {
                StartComputerTurnIfNeeded();
                return;
            }

            _computerTurnMessage = null;
            _selectedSquare = null;
            _selectedMoves = [];

            if (!_game.Resign(side))
            {
                StartComputerTurnIfNeeded();
                return;
            }

            RenderGame(false);
            await PersistGameAsync().ConfigureAwait(true);
        }
        finally
        {
            _isGameActionPending = false;
            UpdateHistoryCommandState();
        }
    }

    private async Task CompleteHistoryNavigationAsync()
    {
        _selectedSquare = null;
        _selectedMoves = [];
        RenderGame(false);
        await PersistGameAsync().ConfigureAwait(true);
        StartComputerTurnIfNeeded();
    }

    private async Task PersistGameAsync()
    {
        if (_gameMode is not GameMode mode)
        {
            return;
        }

        bool wasSaved = await GameSaveService.SaveOrClearAsync(
            _game,
            mode).ConfigureAwait(true);

        if (wasSaved)
        {
            if (_hasSaveFailure)
            {
                _hasSaveFailure = false;
                _computerTurnMessage = null;
                UpdateComputerTurnPresentation();
            }

            return;
        }

        _hasSaveFailure = true;
        _computerTurnMessage =
            LocalizationResources.GetString("GameSaveFailed");
        UpdateComputerTurnPresentation();
    }

    private void SelectOrClearSquare(BoardSquare square)
    {
        BoardPiece? piece = _game.GetPiece(square);

        if (piece is BoardPiece boardPiece &&
            boardPiece.Side == _game.SideToMove)
        {
            SelectSquare(square);
        }
        else
        {
            ClearSelection();
        }
    }

    private async Task<ChessMove?> ResolveMoveAsync(
        ChessMove[] matchingMoves)
    {
        if (matchingMoves.Length == 1)
        {
            return matchingMoves[0];
        }

        PieceSide promotionSide =
            _game.GetPiece(matchingMoves[0].From) is BoardPiece movingPiece
                ? movingPiece.Side
                : _game.SideToMove;
        PromotionChoiceDialog dialog = new(
            XamlRoot,
            matchingMoves,
            promotionSide);

        return await dialog.ShowAsync().ConfigureAwait(true);
    }

    private void SelectSquare(BoardSquare square)
    {
        _selectedSquare = square;
        _selectedMoves = _game.GetLegalMoves(square);
        UpdateHighlightsAndAutomation();
    }

    private void ClearSelection()
    {
        _selectedSquare = null;
        _selectedMoves = [];
        UpdateHighlightsAndAutomation();
    }

    private void RenderGame(bool force)
    {
        for (int rank = 0; rank < BoardSpecification.Ranks; rank++)
        {
            for (int file = 0; file < BoardSpecification.Files; file++)
            {
                BoardSquare square = new(rank, file);
                RenderSquare(square, force);
            }
        }

        RenderCapturedPieces(
            PieceSide.White,
            CapturedByWhiteGrid,
            CapturedByWhiteEmptyText);
        RenderCapturedPieces(
            PieceSide.Black,
            CapturedByBlackGrid,
            CapturedByBlackEmptyText);
        UpdateStatusText();
        UpdateHighlightsAndAutomation();
        UpdateHistoryCommandState();
    }

    private void UpdateHistoryCommandState()
    {
        bool canNavigateHistory = !_game.IsGameOver;

        ((ChessApplication)Application.Current)
            .MainWindow
            .UpdateGameCommandState(
                _isActivePage,
                !_isGameActionPending &&
                    canNavigateHistory &&
                    _game.CanUndo,
                !_isGameActionPending &&
                    canNavigateHistory &&
                    _game.CanRedo,
                _gameMode.HasValue &&
                    !_isGameActionPending);

        UpdateResignationCommandState();
    }

    private void UpdateResignationCommandState()
    {
        bool isLocalGame =
            _gameMode == GameMode.LocalTwoPlayer;
        bool isComputerGame =
            _gameMode == GameMode.Computer;
        bool canResign =
            !_isGameActionPending &&
            !_game.IsGameOver &&
            (isLocalGame || isComputerGame);

        WhiteResignButton.Content = isComputerGame
            ? LocalizationResources.GetString("ResignButton")
            : LocalizationResources.Format(
                "ResignSideButtonFormat",
                ChessText.GetSideName(PieceSide.White));
        BlackResignButton.Content = LocalizationResources.Format(
            "ResignSideButtonFormat",
            ChessText.GetSideName(PieceSide.Black));

        WhiteResignButton.Visibility =
            isLocalGame || isComputerGame
                ? Visibility.Visible
                : Visibility.Collapsed;
        BlackResignButton.Visibility = isLocalGame
            ? Visibility.Visible
            : Visibility.Collapsed;
        WhiteResignButton.IsEnabled = canResign;
        BlackResignButton.IsEnabled =
            canResign && isLocalGame;
    }

    private void RenderSquare(BoardSquare square, bool force)
    {
        BoardPiece? piece = _game.GetPiece(square);

        if (!force &&
            _renderedPieces[square.Rank][square.File] == piece)
        {
            return;
        }

        Grid content = _squareContents[square.Rank][square.File];
        content.Children.Clear();

        if (piece is BoardPiece boardPiece)
        {
            content.Children.Add(new ChessPieceView(boardPiece));
        }

        _renderedPieces[square.Rank][square.File] = piece;
    }

    private void RenderCapturedPieces(
        PieceSide capturingSide,
        Grid targetGrid,
        TextBlock emptyText)
    {
        IReadOnlyList<BoardPiece> capturedPieces =
            _game.GetCapturedPieces(capturingSide);

        targetGrid.Children.Clear();
        targetGrid.RowDefinitions.Clear();
        targetGrid.ColumnDefinitions.Clear();

        emptyText.Visibility = capturedPieces.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        targetGrid.Visibility = capturedPieces.Count == 0
            ? Visibility.Collapsed
            : Visibility.Visible;

        if (capturedPieces.Count == 0)
        {
            return;
        }

        for (int column = 0;
             column < CapturedPieceColumnCount;
             column++)
        {
            targetGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        int rowCount =
            (capturedPieces.Count + CapturedPieceColumnCount - 1) /
            CapturedPieceColumnCount;

        for (int row = 0; row < rowCount; row++)
        {
            targetGrid.RowDefinitions.Add(new RowDefinition());
        }

        double pieceSize = GetDisplayedBoardSquareSize();

        for (int index = 0; index < capturedPieces.Count; index++)
        {
            ChessPieceView pieceView = new(capturedPieces[index])
            {
                Width = pieceSize,
                Height = pieceSize,
                HorizontalAlignment = HorizontalAlignment.Center,
                IsHitTestVisible = false
            };

            Grid.SetRow(pieceView, index / CapturedPieceColumnCount);
            Grid.SetColumn(pieceView, index % CapturedPieceColumnCount);
            targetGrid.Children.Add(pieceView);
        }
    }

    private void OnBoardViewboxSizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(e);

        UpdateCapturedPieceSizes(CapturedByWhiteGrid);
        UpdateCapturedPieceSizes(CapturedByBlackGrid);
    }

    private void UpdateCapturedPieceSizes(Grid targetGrid)
    {
        double pieceSize = GetDisplayedBoardSquareSize();

        foreach (UIElement child in targetGrid.Children)
        {
            if (child is ChessPieceView pieceView)
            {
                pieceView.Width = pieceSize;
                pieceView.Height = pieceSize;
            }
        }
    }

    private double GetDisplayedBoardSquareSize()
    {
        if (BoardViewbox.ActualWidth <= 0 ||
            BoardViewbox.ActualHeight <= 0 ||
            BoardFrame.ActualWidth <= 0 ||
            BoardFrame.ActualHeight <= 0 ||
            BoardGrid.ActualWidth <= 0)
        {
            return DefaultCapturedPieceSize;
        }

        double horizontalScale =
            BoardViewbox.ActualWidth / BoardFrame.ActualWidth;
        double verticalScale =
            BoardViewbox.ActualHeight / BoardFrame.ActualHeight;
        double viewboxScale = Math.Min(horizontalScale, verticalScale);

        return BoardGrid.ActualWidth /
            BoardSpecification.Files *
            viewboxScale;
    }

    private void UpdateStatusText()
    {
        if (_isComputerThinking)
        {
            GameStatusText.Text =
                LocalizationResources.GetString(
                    "StatusComputerThinking");
            return;
        }

        if (_computerTurnMessage is not null)
        {
            GameStatusText.Text = _computerTurnMessage;
            return;
        }

        if (_game.ResignedSide is PieceSide resignedSide)
        {
            GameStatusText.Text = LocalizationResources.Format(
                "StatusResignedFormat",
                ChessText.GetSideName(resignedSide),
                ChessText.GetSideName(
                    _game.Winner ??
                    throw new InvalidOperationException(
                        "A resigned game must have a winner.")));
            return;
        }

        GameStatusText.Text = _game.Status switch
        {
            GameStatus.Active => LocalizationResources.Format(
                "StatusActiveFormat",
                ChessText.GetSideName(_game.SideToMove)),
            GameStatus.Check => LocalizationResources.Format(
                "StatusCheckFormat",
                ChessText.GetSideName(_game.SideToMove)),
            GameStatus.Checkmate =>
                LocalizationResources.Format(
                    "StatusCheckmateFormat",
                    ChessText.GetSideName(
                        _game.Winner ??
                        throw new InvalidOperationException(
                            "A checkmate position must have a winner."))),
            GameStatus.Stalemate =>
                LocalizationResources.GetString("StatusStalemate"),
            GameStatus.DrawByThreefoldRepetition =>
                LocalizationResources.GetString(
                    "StatusDrawByThreefoldRepetition"),
            GameStatus.DrawByFiftyMoveRule =>
                LocalizationResources.GetString(
                    "StatusDrawByFiftyMoveRule"),
            GameStatus.DrawByInsufficientMaterial =>
                LocalizationResources.GetString(
                    "StatusDrawByInsufficientMaterial"),
            _ => throw new ArgumentOutOfRangeException(
                nameof(_game.Status),
                _game.Status,
                "Unknown game status.")
        };
    }

    private void UpdateModeText()
    {
        ModeText.Text = _gameMode switch
        {
            GameMode.LocalTwoPlayer =>
                LocalizationResources.GetString("ModeLocalTwoPlayer"),
            GameMode.Computer =>
                LocalizationResources.GetString("ModeComputer"),
            null =>
                LocalizationResources.GetString("ModeChooseFromHome"),
            _ => LocalizationResources.GetString("ModeNewGame")
        };
    }

    private void StartComputerTurnIfNeeded()
    {
        if (!ShouldComputerMove() ||
            _computerTurnTask is { IsCompleted: false })
        {
            UpdateComputerTurnPresentation();
            return;
        }

        ChessGame liveGame = _game;
        ChessGame analysisSnapshot = liveGame.Clone();
        CancellationTokenSource cancellation = new();
        long sessionId = _gameSessionId;

        _computerTurnCancellation = cancellation;
        _computerTurnMessage = null;
        _isComputerThinking = true;
        UpdateComputerTurnPresentation();

        _computerTurnTask = RunComputerTurnAsync(
            liveGame,
            analysisSnapshot,
            sessionId,
            cancellation);
    }

    private async Task RunComputerTurnAsync(
        ChessGame liveGame,
        ChessGame analysisSnapshot,
        long sessionId,
        CancellationTokenSource cancellation)
    {
        try
        {
            CancellationToken cancellationToken = cancellation.Token;
            ComputerMoveDecision decision = await Task.Run(
                () => _computerOpponent.ChooseMove(
                    analysisSnapshot,
                    ComputerSearchLimits.Standard,
                    cancellationToken),
                cancellationToken).ConfigureAwait(true);

            if (!IsCurrentComputerTurn(
                    liveGame,
                    sessionId,
                    cancellation,
                    cancellationToken))
            {
                return;
            }

            if (!decision.HasMove ||
                decision.Move is not ChessMove move)
            {
                _computerTurnMessage =
                    LocalizationResources.GetString(
                        "ComputerNoLegalMove");
                return;
            }

            if (!await TryApplyMoveAsync(move).ConfigureAwait(true))
            {
                _computerTurnMessage =
                    LocalizationResources.GetString(
                        "ComputerMoveNoLongerLegal");
            }
        }
        catch (OperationCanceledException)
            when (cancellation.IsCancellationRequested)
        {
        }
        catch (ArgumentException)
        {
            ReportComputerFailure(
                liveGame,
                sessionId,
                cancellation);
        }
        catch (InvalidOperationException)
        {
            ReportComputerFailure(
                liveGame,
                sessionId,
                cancellation);
        }
        finally
        {
            cancellation.Dispose();

            if (sessionId == _gameSessionId &&
                ReferenceEquals(_computerTurnCancellation, cancellation))
            {
                _computerTurnCancellation = null;
                _computerTurnTask = null;
                _isComputerThinking = false;
                UpdateComputerTurnPresentation();
            }
        }
    }

    private bool IsCurrentComputerTurn(
        ChessGame liveGame,
        long sessionId,
        CancellationTokenSource cancellation,
        CancellationToken cancellationToken)
    {
        return sessionId == _gameSessionId &&
            ReferenceEquals(_computerTurnCancellation, cancellation) &&
            !cancellationToken.IsCancellationRequested &&
            ReferenceEquals(_game, liveGame) &&
            ShouldComputerMove();
    }

    private bool ShouldComputerMove()
    {
        return _gameMode == GameMode.Computer &&
            !_game.IsGameOver &&
            _game.SideToMove == PieceSide.Black;
    }

    private bool CanHumanUseBoard()
    {
        return !_game.IsGameOver &&
            !_isComputerThinking &&
            (_gameMode == GameMode.LocalTwoPlayer ||
                (_gameMode == GameMode.Computer &&
                    _game.SideToMove == PieceSide.White));
    }

    private void CancelComputerTurn()
    {
        _gameSessionId++;

        CancellationTokenSource? cancellation =
            _computerTurnCancellation;
        _computerTurnCancellation = null;
        _computerTurnTask = null;
        _isComputerThinking = false;
        cancellation?.Cancel();

        UpdateComputerTurnPresentation();
    }

    private void ReportComputerFailure(
        ChessGame liveGame,
        long sessionId,
        CancellationTokenSource cancellation)
    {
        if (sessionId == _gameSessionId &&
            ReferenceEquals(_computerTurnCancellation, cancellation) &&
            ReferenceEquals(_game, liveGame))
        {
            _computerTurnMessage =
                LocalizationResources.GetString(
                    "ComputerOpponentError");
        }
    }

    private void UpdateComputerTurnPresentation()
    {
        ComputerThinkingIndicator.IsActive = _isComputerThinking;
        ComputerThinkingIndicator.Visibility = _isComputerThinking
            ? Visibility.Visible
            : Visibility.Collapsed;
        UpdateStatusText();
        UpdateBoardInputState();
        UpdateSquareAutomationNames();
    }

    private void UpdateBoardInputState()
    {
        bool isEnabled = _game.IsGameOver || CanHumanUseBoard();

        for (int rank = 0; rank < BoardSpecification.Ranks; rank++)
        {
            for (int file = 0; file < BoardSpecification.Files; file++)
            {
                _squareInputs[rank][file].IsEnabled = isEnabled;
            }
        }
    }

    private void UpdateHighlightsAndAutomation()
    {
        ResetHighlights();
        HighlightCheckedKing();

        if (_selectedSquare is BoardSquare selectedSquare)
        {
            Border selectedHighlight =
                _squareHighlights[selectedSquare.Rank][selectedSquare.File];
            selectedHighlight.BorderBrush = SelectedSquareBrush;
            selectedHighlight.BorderThickness = new Thickness(4);
        }

        foreach (IGrouping<BoardSquare, ChessMove> destination in
                 _selectedMoves.GroupBy(static move => move.To))
        {
            bool isCapture = destination.Any(static move => move.IsCapture);
            bool isMinisterExchange = destination.Any(
                static move => move.Kind == MoveKind.MinisterExchange);
            Border targetHighlight =
                _squareHighlights[
                    destination.Key.Rank][destination.Key.File];
            targetHighlight.Child = CreateMoveMarker(
                isCapture,
                isMinisterExchange);
        }

        UpdateBoardInputState();
        UpdateSquareAutomationNames();
    }

    private void ResetHighlights()
    {
        for (int rank = 0; rank < BoardSpecification.Ranks; rank++)
        {
            for (int file = 0; file < BoardSpecification.Files; file++)
            {
                Border highlight = _squareHighlights[rank][file];
                highlight.BorderBrush = null;
                highlight.BorderThickness = new Thickness(0);
                highlight.Child = null;
            }
        }
    }

    private void HighlightCheckedKing()
    {
        if (_game.Status is not
            GameStatus.Check and not GameStatus.Checkmate)
        {
            return;
        }

        foreach (PiecePlacement placement in _game.GetPieces())
        {
            if (placement.Piece.Kind == PieceKind.King &&
                placement.Piece.Side == _game.SideToMove)
            {
                Border highlight =
                    _squareHighlights[
                        placement.Square.Rank][placement.Square.File];
                highlight.BorderBrush = CheckedKingBrush;
                highlight.BorderThickness = new Thickness(4);
                return;
            }
        }
    }

    private void UpdateSquareAutomationNames()
    {
        HashSet<BoardSquare> legalDestinations = [.. _selectedMoves.Select(static move => move.To)];
        HashSet<BoardSquare> captureDestinations = [.. _selectedMoves
            .Where(static move => move.IsCapture)
            .Select(static move => move.To)];
        HashSet<BoardSquare> ministerExchangeDestinations = [.. _selectedMoves
            .Where(static move => move.Kind == MoveKind.MinisterExchange)
            .Select(static move => move.To)];

        for (int rank = 0; rank < BoardSpecification.Ranks; rank++)
        {
            for (int file = 0; file < BoardSpecification.Files; file++)
            {
                BoardSquare square = new(rank, file);
                Button input = _squareInputs[rank][file];
                string name = CreateSquareAutomationName(
                    square,
                    legalDestinations,
                    captureDestinations,
                    ministerExchangeDestinations);

                AutomationProperties.SetName(input, name);
                AutomationProperties.SetHelpText(
                    input,
                    CreateSquareAutomationHelpText(
                        square,
                        ministerExchangeDestinations));
                ToolTipService.SetToolTip(input, name);
            }
        }
    }

    private string CreateSquareAutomationName(
        BoardSquare square,
        HashSet<BoardSquare> legalDestinations,
        HashSet<BoardSquare> captureDestinations,
        HashSet<BoardSquare> ministerExchangeDestinations)
    {
        BoardPiece? piece = _game.GetPiece(square);
        string coordinate =
            LocalizationResources.IsolateBidirectionalText(
                square.Notation.ToUpperInvariant());
        string? description = piece is BoardPiece boardPiece
            ? ChessText.GetPieceDescription(boardPiece)
            : null;
        string? state = null;

        if (_selectedSquare == square)
        {
            state = LocalizationResources.GetString(
                "SquareStateSelected");
        }
        else if (ministerExchangeDestinations.Contains(square))
        {
            state = LocalizationResources.GetString(
                "SquareStateLegalMinisterExchange");
        }
        else if (captureDestinations.Contains(square))
        {
            state = LocalizationResources.GetString(
                "SquareStateLegalCapture");
        }
        else if (legalDestinations.Contains(square))
        {
            state = LocalizationResources.GetString(
                "SquareStateLegalDestination");
        }

        if (state is not null)
        {
            description = description is null
                ? state
                : LocalizationResources.Format(
                    "SquareContentAndStateFormat",
                    description,
                    state);
        }

        return description is null
            ? coordinate
            : LocalizationResources.Format(
                "SquareAutomationNameFormat",
                coordinate,
                description);
    }

    private string CreateSquareAutomationHelpText(
        BoardSquare square,
        HashSet<BoardSquare> ministerExchangeDestinations)
    {
        if (_game.IsGameOver)
        {
            return LocalizationResources.GetString(
                "SquareHelpGameOver");
        }

        if (_isComputerThinking)
        {
            return LocalizationResources.GetString(
                "SquareHelpComputerThinking");
        }

        if (_gameMode == GameMode.Computer &&
            _game.SideToMove == PieceSide.Black)
        {
            return LocalizationResources.GetString(
                "SquareHelpComputerControlsBlack");
        }

        if (ministerExchangeDestinations.Contains(square) &&
            _game.GetPiece(square) is BoardPiece royalPiece)
        {
            return LocalizationResources.Format(
                "SquareHelpMinisterExchangeFormat",
                ChessText.GetPieceDescription(royalPiece));
        }

        return LocalizationResources.GetString(
            "SquareHelpSelect");
    }

    private static UIElement CreateMoveMarker(
        bool isCapture,
        bool isMinisterExchange)
    {
        if (isMinisterExchange)
        {
            return new Border
            {
                Width = 44,
                Height = 44,
                Background = TransparentBrush,
                BorderBrush = MinisterExchangeBrush,
                BorderThickness = new Thickness(3),
                CornerRadius = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = "⇄",
                    FontSize = 24,
                    Foreground = MinisterExchangeBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }

        return isCapture
            ? new Ellipse
            {
                Width = 42,
                Height = 42,
                Fill = TransparentBrush,
                Stroke = CaptureMoveBrush,
                StrokeThickness = 4,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
            : new Ellipse
            {
                Width = 14,
                Height = 14,
                Fill = LegalMoveBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
    }

    private static SolidColorBrush GetBaseSquareBrush(BoardSquare square)
    {
        return (square.Rank + square.File) % 2 == 0
            ? LightSquareBrush
            : DarkSquareBrush;
    }

    private static Grid[][] CreateSquareContents()
    {
        Grid[][] contents = new Grid[BoardSpecification.Ranks][];

        for (int rank = 0; rank < BoardSpecification.Ranks; rank++)
        {
            contents[rank] = new Grid[BoardSpecification.Files];
        }

        return contents;
    }

    private static Border[][] CreateSquareHighlights()
    {
        Border[][] highlights = new Border[BoardSpecification.Ranks][];

        for (int rank = 0; rank < BoardSpecification.Ranks; rank++)
        {
            highlights[rank] = new Border[BoardSpecification.Files];
        }

        return highlights;
    }

    private static Button[][] CreateSquareInputs()
    {
        Button[][] inputs = new Button[BoardSpecification.Ranks][];

        for (int rank = 0; rank < BoardSpecification.Ranks; rank++)
        {
            inputs[rank] = new Button[BoardSpecification.Files];
        }

        return inputs;
    }

    private static BoardPiece?[][] CreateRenderedPieces()
    {
        BoardPiece?[][] pieces =
            new BoardPiece?[BoardSpecification.Ranks][];

        for (int rank = 0; rank < BoardSpecification.Ranks; rank++)
        {
            pieces[rank] =
                new BoardPiece?[BoardSpecification.Files];
        }

        return pieces;
    }
}
