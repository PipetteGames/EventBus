# EventBus

Unity で利用可能な汎用的な EventBus パターン実装パッケージです。イベントの発行・購読をシンプルに管理できます。

## 特徴

- **シンプルな API**: 基本的な Subscribe/Publish で簡単に使用可能。
- **拡張性**: ExecutionOrder で実行順を制御、Filter で条件付き実行。
- **パフォーマンス最適化**: GC アロケーションを最小限に抑え、マルチスレッド対応。
- **型安全**: ジェネリックを使用し、コンパイル時エラー検出。
- **Unity 対応**: Unity Package Manager でインストール可能。

## インストール

1. Unity Package Manager を開く。
2. 「Add package from git URL」を選択。
3. URL: `https://github.com/PipetteGames/EventBus.git?path=Packages/EventBus` を入力。
4. インストール完了。

または、Packages/manifest.json に以下を追加:

```json
{
  "dependencies": {
    "com.pipettegames.eventbus": "https://github.com/PipetteGames/EventBus.git?path=Packages/EventBus"
  }
}
```

### 特定のバージョンを使用したい場合

URL の末尾にバージョン指定を追加

例: `https://github.com/PipetteGames/EventBus.git?path=Packages/EventBus#v0.3.0`

## 基本的な使い方

### 1. イベント構造体の定義

```csharp
using PipetteGames.Events.Interfaces;

public struct PlayerDiedEvent : IEvent
{
    public string PlayerName { get; set; }
    public int Score { get; set; }
}
```

### 2. EventBus のインスタンス作成

```csharp
using PipetteGames.Events;
using PipetteGames.Events.Interfaces;

public class GameManager : MonoBehaviour
{
    private IEventBus _eventBus = new EventBus();
}
```

### 3. イベントの購読

```csharp
void Start()
{
    // イベント購読
    _eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
}

// イベントハンドラ
private void OnPlayerDied(PlayerDiedEvent e)
{
    Debug.Log($"{e.PlayerName} が死亡しました！");
}
```

### 4. イベントの発行

```csharp
// イベント発行
_eventBus.Publish(new PlayerDiedEvent { PlayerName = "Player1" });
```

### 5. 購読解除

Subscribe は `IEventSubscription` を返します。これを Dispose することで購読解除できます。

```csharp
private IEventSubscription _subscription;

void Start()
{
    // 購読し、IEventSubscription を保持
    _subscription = _eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
}

void OnDestroy()
{
    // Dispose で購読解除
    _subscription?.Dispose();
}
```

または、直接 Unsubscribe メソッドを使用:

```csharp
void OnDestroy()
{
    // Unsubscribe で購読解除
    _eventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
}
```

## 高度な使い方

### ExecutionOrder（実行順制御）

ハンドラーの実行順を指定。値が小さいほど先に実行されます。

```csharp
// ExecutionOrder = 0（デフォルト）
_eventBus.Subscribe<PlayerDiedEvent>(handler1, executionOrder: 0);

// ExecutionOrder = 10（後に実行）
_eventBus.Subscribe<PlayerDiedEvent>(handler2, executionOrder: 10);
```

### Filter（条件付き実行）

イベントデータを基に、ハンドラーの実行をフィルタリング。

```csharp
// スコアが100以上の場合のみ実行
_eventBus.Subscribe<PlayerDiedEvent>(
    e => Debug.Log($"{e.PlayerName} が高スコアを達成しました！ スコア: {e.Score}"),
    filter: e => e.Score > 100
);
```

### 組み合わせ

ExecutionOrder と Filter を組み合わせ。

```csharp
_eventBus.Subscribe<PlayerDiedEvent>(
    handler,
    executionOrder: 5,
    filter: e => e.Score > 50
);
```

## 非同期イベント（Async/Await 対応）

### 非同期イベント構造体の定義

`IAwaitableEvent` インターフェースを実装して、非同期対応イベントを定義します。

```csharp
using PipetteGames.Events.Interfaces;

public struct DataLoadedEvent : IAwaitableEvent
{
    public string Data { get; set; }
}
```

### 非同期ハンドラーの購読

非同期ハンドラーは `Func<T, Task>` を使用して購読します。

```csharp
void Start()
{
    // 非同期ハンドラーを購読
    _eventBus.Subscribe<DataLoadedEvent>(OnDataLoaded);
    
    // 実行順を指定して購読
    _eventBus.Subscribe<DataLoadedEvent>(OnDataLoadedOrdered, executionOrder: 5);
}

// 非同期ハンドラー
private async Task OnDataLoaded(DataLoadedEvent e)
{
    // 非同期処理を実行
    await LoadRemoteData(e.Data);
    Debug.Log($"データ読み込み完了: {e.Data}");
}

private async Task OnDataLoadedOrdered(DataLoadedEvent e)
{
    await ProcessData(e.Data);
}
```

### 非同期イベントの発行

`PublishAsync` メソッドでイベントを発行し、すべてのハンドラーの完了を待ちます。

```csharp
public async Task PublishDataEvent()
{
    // すべての非同期ハンドラーの完了を待つ
    await _eventBus.PublishAsync(new DataLoadedEvent { Data = "sample.json" });
    
    Debug.Log("すべてのハンドラーが完了しました");
}
```

### UniTask 対応

プロジェクトで [UniTask](https://github.com/Cysharp/UniTask) を使用している場合、Project Settings の Script Define Symbols に `EVENTBUS_UNITASK_SUPPORT` を追加することで、`UniTask` 型を利用することができます。
追加していない場合は、標準の `Task` 型を使用します。

**非同期イベント処理には、UniTask の使用を推奨します。** UniTask は Unity の非同期処理に最適化されており、パフォーマンスと互換性の面で優れています。

```csharp
// UniTask対応が有効な場合
private async UniTask OnDataLoaded(DataLoadedEvent e)
{
    await UniTask.Delay(500);
    Debug.Log("データ読み込み完了");
}

// 発行時
public async UniTask PublishDataEvent()
{
    await _eventBus.PublishAsync(new DataLoadedEvent { Data = "sample" });
}
```

## 簡易 API リファレンス

### IEventBus インターフェース

#### 同期イベント

```csharp
public interface IEventBus
{
    public IEventSubscription Subscribe<T>(Action<T> handler) where T : IEvent;
    public IEventSubscription Subscribe<T>(Action<T> handler, int executionOrder) where T : IEvent;
    public IEventSubscription Subscribe<T>(Action<T> handler, Func<T, bool> filter) where T : IEvent;
    public IEventSubscription Subscribe<T>(Action<T> handler, int executionOrder, Func<T, bool> filter) where T : IEvent;
    public void Unsubscribe<T>(Action<T> handler) where T : IEvent;
    public void Publish<T>(T eventData) where T : IEvent;
}
```

#### 非同期イベント

**EVENTBUS_UNITASK_SUPPORT が定義されている場合:**

```csharp
public IEventSubscription Subscribe<T>(Func<T, UniTask> handler) where T : IAwaitableEvent;
public IEventSubscription Subscribe<T>(Func<T, UniTask> handler, int executionOrder) where T : IAwaitableEvent;
public void Unsubscribe<T>(Func<T, UniTask> handler) where T : IAwaitableEvent;
public UniTask PublishAsync<T>(T eventData) where T : IAwaitableEvent;
```

**EVENTBUS_UNITASK_SUPPORT が定義されていない場合:**

```csharp
public IEventSubscription Subscribe<T>(Func<T, Task> handler) where T : IAwaitableEvent;
public IEventSubscription Subscribe<T>(Func<T, Task> handler, int executionOrder) where T : IAwaitableEvent;
public void Unsubscribe<T>(Func<T, Task> handler) where T : IAwaitableEvent;
public Task PublishAsync<T>(T eventData) where T : IAwaitableEvent;
```

### IEvent インターフェース

イベントクラスが実装するマーカーインターフェース。

```csharp
public interface IEvent { }
```

### IAwaitableEvent インターフェース

非同期イベントクラスが実装するマーカーインターフェース。`IEvent` を継承しています。

```csharp
public interface IAwaitableEvent : IEvent { }
```

### IEventSubscription インターフェース

購読解除用のインターフェース。

```csharp
public interface IEventSubscription : IDisposable { }
```

## 注意点

### 同期イベント

- **マルチスレッド**: 基本的にシングルスレッド推奨。マルチスレッド使用時は内部 lock で安全。
- **パフォーマンス**: 購読/購読解除時に実行順ソートが走るため低頻度推奨。頻繁にイベントの有効化/無効化を行う場合は Subscribe/Unsubscribe を使用せず、ハンドラ内で制御を推奨。 Publish は高速。
- **メモリリーク**: Dispose または Unsubscribe を忘れずに。

### 非同期イベント

- **実行順序**: `PublishAsync` はハンドラーを**順序通りに** 実行し、すべての完了を待ちます。複数のハンドラーは**並行ではなく順序通り**に実行される点に注意してください。
- **例外処理**: ハンドラー内での例外は `Console.WriteLine(ex)` で記録され、他のハンドラーの実行は継続します。
- **キャンセル**: `PublishAsync` 中にキャンセルトークンは渡せません。必要に応じてハンドラー内で処理してください。
- **UniTask 対応**: UniTask を使用するには、Project Settings の Script Define Symbols に `EVENTBUS_UNITASK_SUPPORT` を追加してください。

## ライセンス

MIT License
