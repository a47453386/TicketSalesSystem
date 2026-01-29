 // POST: Programmes/Delete/5
 [HttpPost, ActionName("Delete")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> DeleteConfirmed(string id)
 {
     var programme = await _context.Programme.FindAsync(id);
     if (programme != null)
     {
         _context.Programme.Remove(programme);
     }

     await _context.SaveChangesAsync();
     return RedirectToAction(nameof(Index));
 }