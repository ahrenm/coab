using Classes;

namespace engine
{
    class seg040
    {
        internal static DaxBlock LoadDax(byte mask_colour, byte masked, int block_id, string fileName) // load_dax
        {
            short pic_size;
            byte[] pic_data;
            seg042.load_decode_dax(out pic_data, out pic_size, block_id, fileName + ".dax");

            if (pic_size != 0)
            {
                int height = Sys.ArrayToShort(pic_data, 0);
                int width = Sys.ArrayToShort(pic_data, 2);
                int x_pos = Sys.ArrayToShort(pic_data, 4);
                int y_pos = Sys.ArrayToShort(pic_data, 6);
                int item_count = pic_data[8];

                DaxBlock mem_ptr = new DaxBlock(masked, item_count, width, height);
                System.Array.Copy(pic_data, 9, mem_ptr.field_9, 0, 8);

                int pic_data_offset = 17;

                turn_dax_to_videolayout(mem_ptr, mask_colour, masked, pic_data_offset, pic_data);

                seg043.clear_keyboard();

                return mem_ptr;
            }

            return null;
        }

        internal static void turn_dax_to_videolayout(DaxBlock dax_block, byte mask_colour, byte masked, int block_offset, byte[] data)
        {
            /* TODO, this function needs to account for masked colours */

            if (dax_block != null)
            {
                int dest_offset = 0;

                for (int loop1_var = 1; loop1_var <= dax_block.item_count; loop1_var++)
                {
                    int height = dax_block.height - 1;

                    for (int loop2_var = 0; loop2_var <= height; loop2_var++)
                    {
                        int width = (dax_block.width * 4) - 1;

                        for (int loop3_var = 0; loop3_var <= width; loop3_var++)
                        {
                            byte a = (byte)((data[block_offset]) >> 4);
                            byte b = (byte)((data[block_offset]) & 0x0f);

                            if (masked == 1 && a == mask_colour)
                            //if (masked != 0 && a == mask_colour)
                            {
                                dax_block.data[dest_offset] = 16;
                            }
                            else
                            {
                                dax_block.data[dest_offset] = a;
                            }
                            dest_offset += 1;

                            if (masked == 1 && b == mask_colour)
                            //if (masked != 0 && b == mask_colour)
                            {
                                dax_block.data[dest_offset] = 16;
                            }
                            else
                            {
                                dax_block.data[dest_offset] = b;
                            }

                            dest_offset += 1;
                            block_offset += 1;
                        }
                    }
                }
            }
        }


        internal static void OverlayUnbounded(DaxBlock source, int arg_8, int itemIdex, int rowY, int colX)
        {
            draw_combat_picture(source, rowY + 1, colX + 1, itemIdex);
        }


        internal static void OverlayBounded(DaxBlock source, byte arg_8, int itemIndex, int rowY, int colX) /* sub_E353 */
        {
            draw_combat_picture(source, rowY + 1, colX + 1, itemIndex);
        }


        internal static void flipIconLeftToRight(DaxBlock dest, DaxBlock source)
        {
            if (source != null && dest != null)
            {
                System.Array.Copy(source.field_9, dest.field_9, 8);

                int width = source.width * 8;
                for( int y = 0; y < source.height; y++ )
                {
                    for (int x = 0; x < width; x++)
                    {
                        int di = (y * width) + x;
                        int si = (y*width) + (width - x) - 1;
                        byte[] dd = dest.data;
                        dest.data[di] = source.data[si];
                        //dest.data[di+1] = source.data[si+0];

                        if (source.data_ptr != null)
                        {
                            dest.data_ptr[di] = source.data_ptr[si];
                            //dest.data_ptr[di+1] = source.data_ptr[si+0];
                        }
                    }
                }
            }
        }


        internal static void ega_backup(DaxBlock dax_block, int rowY, int colX) /* ega_01 */
        {
            if (dax_block != null)
            {
                int var_10 = 0;

                int minY = rowY * 8;
                int maxY = minY + dax_block.height;

                int minX = colX * 8;
                int maxX = minX + (dax_block.width * 8);

                for (int pixY = minY; pixY < maxY; pixY++)
                {
                    for (int pixX = minX; pixX < maxX; pixX++)
                    {
                        dax_block.data[var_10] = Display.GetPixel(pixX, pixY);
                        var_10++;
                    }
                }
            }
        }

        static byte color_no_draw = 17;
        static byte color_re_color_from = 17;
        static byte color_re_color_to = 17;
        internal static void draw_clipped_recolor(byte from, byte to)
        {
            color_re_color_from = from;
            color_re_color_to = to;
        }

        internal static void draw_clipped_nodraw(byte color)
        {
            color_no_draw = color;
        }

        internal static void draw_clipped_picture(DaxBlock dax_block, int rowY, int colX, int index, 
            int clipMinX, int clipMaxX, int clipMinY, int clipMaxY)
        {
            if (dax_block != null)
            {
                int var_10 = index * dax_block.bpp;

                int minY = rowY * 8;
                int maxY = minY + dax_block.height;

                int minX = colX * 8;
                int maxX = minX + (dax_block.width * 8);

                for (int pixY = minY; pixY < maxY; pixY++)
                {
                    for (int pixX = minX; pixX < maxX; pixX++)
                    {
                        if (pixX >= clipMinX && pixX < clipMaxX && 
                            pixY >= clipMinY && pixY < clipMaxY)
                        {
                            byte color = dax_block.data[var_10];

                            if (color == color_no_draw)
                            { }
                            else if (color == color_re_color_from)
                            {
                                Display.SetPixel3(pixX, pixY, color_re_color_to);
                            }
                            else
                            {
                                Display.SetPixel3(pixX, pixY, color);
                            }
                        }

                        var_10++;
                    }
                }

                Display.Update();
            }
        }

        internal static void draw_combat_picture(DaxBlock dax_block, int rowY, int colX, int index)
        {
            draw_clipped_picture(dax_block, rowY, colX, index, 8, 176, 8, 176);
        }

        internal static void draw_picture(DaxBlock dax_block, int rowY, int colX, int index)
        {
            draw_clipped_picture(dax_block, rowY, colX, index, 0, 320, 0, 200);
        }
        
        //static int backcolor = 0;

        internal static void DrawOverlay()
        {
            //TODO this might be useful when we move to OpenGL.
        }

        internal static void SetPaletteColor(byte color, byte index)
        {
            byte newColor = color;

            if (color >= 8)
            {
                //newColor += 8;
            }

            Display.SetEgaPalette(index, newColor);
        }

        internal static void DaxBlockRecolor(DaxBlock dax_block, byte arg_4, short arg_6, byte[] newColors, byte[] oldColors)
        {
            if (dax_block != null)
            {
                int item_count;
                if (arg_6 < 0)
                {
                    arg_6 = 0;
                    item_count = dax_block.item_count;
                }
                else
                {
                    item_count = 1;
                }

                int src_offset = dax_block.bpp * arg_6;
                int dest_offset = 0;
                DaxBlock tmp_block = new DaxBlock(0, item_count, dax_block.width, dax_block.height);

                System.Array.Copy(dax_block.field_9, tmp_block.field_9, dax_block.field_9.Length);
                System.Array.Copy(dax_block.data, src_offset, tmp_block.data, dest_offset, tmp_block.item_count * tmp_block.bpp);

                for (int color_index = 0; color_index <= 15; color_index++)
                {
                    src_offset = dax_block.bpp * arg_6;
                    dest_offset = 0;

                    if (oldColors[color_index] != newColors[color_index])
                    {
                        int var_3F = item_count + arg_6 - 1;

                        for (int block = arg_6; block <= var_3F; block++)
                        {
                            for (int posY = 0; posY < tmp_block.height; posY++)
                            {
                                for (int posX = 0; posX < (tmp_block.width * 8); posX++)
                                {
                                    if (dax_block.data[src_offset] == oldColors[color_index] &&
                                        (arg_4 == 0 || (seg051.Random(4) == 0)))
                                    {
                                        tmp_block.data[dest_offset] = newColors[color_index];
                                    }

                                    src_offset += 1;
                                    dest_offset += 1;
                                }
                            }
                        }
                    }
                }

                System.Array.Copy(tmp_block.data, 0, dax_block.data, dax_block.bpp * arg_6, item_count * dax_block.bpp);
                seg040.free_dax_block(ref tmp_block);
            }
        }

        internal static void free_dax_block(ref DaxBlock block_ptr)
        {
            if (block_ptr != null)
            {
                int var_2 = block_ptr.item_count * block_ptr.bpp;

                if (block_ptr.data_ptr != null)
                {
                    seg051.FreeMem(var_2, block_ptr.data_ptr);
                }

                seg051.FreeMem(var_2 + 0x17, block_ptr);

                block_ptr = null;
            }
        }


        internal static void DrawColorBlock(int color, int lineCount, int colWidth, int lineY, int colX)
        {
            int minY = lineY + 8;
            int maxY = minY + lineCount;

            int minX = (colX * 8) + 8;
            int maxX = minX + (colWidth * 8);

            for (int pixY = minY; pixY < maxY; pixY++)
            {
                for (int pixX = minX; pixX < maxX; pixX++)
                {
                    if (pixX >= 0 && pixX < 320 && pixY >= 0 && pixY < 200)
                    {
                        Display.SetPixel3(pixX, pixY, color);
                    }
                }
            }
        }
    }
}
