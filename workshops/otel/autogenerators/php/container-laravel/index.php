use Illuminate\Support\Facades\Route;

Route::get('/rolldice', function () {
    return response()->json(['roll' => random_int(1, 6)]);
});
